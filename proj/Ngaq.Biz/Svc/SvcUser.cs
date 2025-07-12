using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Ngaq.Biz.Db;
using Ngaq.Biz.Db.User;
using Ngaq.Biz.Infra.Cfg;
using Ngaq.Biz.Tools;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Model.Sys.Po.Password;
using Ngaq.Core.Model.Sys.Po.User;
using Ngaq.Core.Model.Sys.Req;
using Ngaq.Core.Models.Sys.Req;
using Ngaq.Core.Models.Sys.Resp;
using Ngaq.Core.Sys.Svc;
using Ngaq.Core.Tools;
using Ngaq.Local.Db;
using StackExchange.Redis;
using Tsinswreng.CsCfg;
using Tsinswreng.CsCore;
using Tsinswreng.CsSqlHelper;
namespace Ngaq.Biz.Svc;

public class SvcUser(
	DaoUser DaoUser
	,IRepo<PoUser, IdUser> RepoUser
	,IRepo<PoPassword, IdPassword> RepoPassword
	// ,DbFnCtxMkr DbFnCtxMkr
	// ,ITxnRunner TxnRunner
	,TxnWrapper<DbFnCtx> TxnWrapper
	,IConnectionMultiplexer Redis
)
	: ISvcUser
{

public str GeneAccessToken(
		str UserIdStr
	){
		var JwtSecret = ServerCfgItems.Inst.JwtSecret.GetFrom(ServerCfg.Inst);
		var securityKey = new SymmetricSecurityKey(
			//注意: 小於256字節則報錯
			//Encoding.UTF8.GetBytes("2025-04-16T21:00:39.328+08:00_W16-3=2025-04-16T21:00:50.706+08:00_W16-3")
			Encoding.UTF8.GetBytes(JwtSecret??"")
		);
		var credentials = new SigningCredentials(
			securityKey, SecurityAlgorithms.HmacSha256Signature
		);
		var claims = new[]{
			//subject, 标识令牌的归属实体（如用户、服务或设备）
			new Claim(JwtRegisteredClaimNames.Sub, UserIdStr)
			//Jwt Id 为令牌提供全局唯一标识符，防范重放攻击（Replay Attack）
			,new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()??"")
			//issuedAt
			,new Claim(
				JwtRegisteredClaimNames.Iat
				,DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
				,ClaimValueTypes.Integer64
			)
			//,new Claim("role", "admin")//custom
		};
		var token = new JwtSecurityToken(
			issuer: "service-alpha-dev"
			,audience: "client-app-dev"
			,claims: claims
			,expires: DateTime.UtcNow.AddMinutes(30)
			,signingCredentials: credentials
		);
		var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
		return accessToken;
	}



	public async Task<Func<
		ReqAddUser
		,CT
		,Task<nil>
	>> FnAddUser(
		IDbFnCtx? DbFnCtx
		,CT Ct
	){
		var AddUsers = await RepoUser.FnInsertMany(DbFnCtx, Ct);
		var AddPasswords = await RepoPassword.FnInsertMany(DbFnCtx, Ct);
		var Fn = async(ReqAddUser ReqAddUser, CT Ct)=>{
			//TODO校驗
			var Id = new IdUser();
			var User = new PoUser{
				Id = Id
				,UniqueName = ReqAddUser.UniqueName??Id.ToString()
				,Email = ReqAddUser.Email
			};
			var PasswordHash = await ToolArgon.Inst.HashPasswordAsy(ReqAddUser.Password, Ct);
			var Password = new PoPassword{
				Id = new()
				,UserId = User.Id
				,Algo = (i64)PoPassword.EAlgo.Argon2id
				,Text = PasswordHash
			};
			await AddUsers([User], Ct);
			await AddPasswords([Password], Ct);
			return NIL;
		};
		return Fn;
	}


	public async Task<Func<
		ReqLogin
		,CT
		,Task<RespLogin>
	>> FnLogin(
		IDbFnCtx DbFnCtx
		,CT Ct
	){
		//var SelectUserById = await RepoUser.FnSelectByIdAsy(DbFnCtx, ct);
		var SelectUserByUniqueName = await DaoUser.FnSelectByUniqueName(DbFnCtx, Ct);
		var SelectUserByEmail = await DaoUser.FnSelectByEmail(DbFnCtx, Ct);
		var SelectPasswordById = await DaoUser.FnSelectPasswordById(DbFnCtx, Ct);
		var Fn = async(ReqLogin Req, CT Ct)=>{
			//TODO 校驗Req
			PoUser? PoUser = null;
			if(Req.UserIdentityMode == (i64)ReqLogin.EUserIdentityMode.UniqueName){
				if(str.IsNullOrEmpty(Req.UniqueName)){
					throw new ErrArg("str.IsNullOrEmpty(Req.UniqueName)"); //TODO 優化異常處理 錯誤碼, 前端多語言
				}
				PoUser = await SelectUserByUniqueName(Req.UniqueName,Ct);
			}else if(Req.UserIdentityMode == (i64)ReqLogin.EUserIdentityMode.Email){
				if(str.IsNullOrEmpty(Req.Email)){
					throw new ErrArg("str.IsNullOrEmpty(Req.Email)"); //TODO 優化異常處理 錯誤碼, 前端多語言
				}
				PoUser = await SelectUserByEmail(Req.Email,Ct);
			}
			if(PoUser == null){
				throw new ErrBase("User not exsists");
			}

			var PoPassword = await SelectPasswordById(PoUser.Id, Ct);
			if(!(	await ToolArgon.Inst.VerifyPasswordAsy(Req.Password??"", PoPassword.Text, Ct)	)){
				throw new ErrBase("Password not correct");
			};

			var RespLogin = new RespLogin{
				AccessToken = GeneAccessToken(PoUser.Id.ToString())
				,PoUser = PoUser
				,UserIdStr = PoUser.Id.ToString()
			};
			return RespLogin;
		};
		return Fn;
	}

	[Impl]
	public async Task<nil> AddUser(ReqAddUser ReqAddUser ,CT Ct){
		return await TxnWrapper.Wrap(FnAddUser, ReqAddUser, Ct);
	}

	[Impl]
	public async Task<RespLogin> Login(ReqLogin ReqLogin ,CT Ct){
		return await TxnWrapper.Wrap(FnLogin, ReqLogin, Ct);
	}

}
