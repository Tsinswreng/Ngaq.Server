namespace Ngaq.Biz.Domains.User.Svc;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Ngaq.Biz.Db.User;
using Ngaq.Biz.Infra.Cfg;
using Ngaq.Biz.Tools;
using Ngaq.Core.Domains.User.Models.Req;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Model.Sys.Po.Password;
using Ngaq.Core.Models.Sys.Po.Password;
using Ngaq.Core.Models.Sys.Po.User;
using Ngaq.Core.Models.Sys.Req;
using Ngaq.Core.Sys.Svc;
using Ngaq.Core.Tools;
using Ngaq.Local.Db.TswG;
using Tsinswreng.CsCfg;
using Tsinswreng.CsCore;
using Tsinswreng.CsSqlHelper;
using Ngaq.Core.Domains.Base.Models.Req;
using Ngaq.Core.Domains.User.Models.Resp;
using Ngaq.Core.Domains.User.Models.Po.User;

public partial class SvcUser(
	DaoUser DaoUser
	,IRepo<PoUser, IdUser> RepoUser
	,IRepo<PoPassword, IdPassword> RepoPassword
	// ,DbFnCtxMkr DbFnCtxMkr
	// ,ITxnRunner TxnRunner
	,TxnWrapper<DbFnCtx> TxnWrapper
	,IDistributedCache Cache
	,ILogger<SvcUser> Log
	,ISvcToken SvcToken
)
	:ISvcUser
{

	public str GeneAccessToken(str UserIdStr){
		return SvcToken.GenAccessToken(UserIdStr);
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
		return async(reqAddUser, Ct)=>{
			reqAddUser.Validate();
			var Id = new IdUser();
			var User = new PoUser{
				Id = Id
				,UniqueName = reqAddUser.UniqueName??Id.ToString()
				,Email = reqAddUser.Email
			};
			var PasswordHash = await ToolArgon.Inst.HashPasswordAsy(reqAddUser.Password, Ct);
			var Password = new PoPassword{
				Id = new()
				,UserId = User.Id
				,Algo = PoPassword.EAlgo.Argon2id
				,Text = PasswordHash
			};
			await AddUsers([User], Ct);
			await AddPasswords([Password], Ct);
			//TODO
			try{
				await Cache.SetStringAsync($"user:register:{User.Id}", JSON.stringify(User), new DistributedCacheEntryOptions {
					AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
				}, Ct);
			}catch(Exception e){
				Log.LogError(e, "Failed to cache user registration info.");
			}
			return NIL;
		};
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
		var SelectPasswordById = await DaoUser.FnSlctPasswordByUserId(DbFnCtx, Ct);
		return async(Req, Ct)=>{
			//TODO 校驗Req
			PoUser? PoUser = null;
			if(Req.UserIdentityMode == ReqLogin.EUserIdentityMode.UniqueName){
				if(str.IsNullOrEmpty(Req.UniqueName)){
					throw new ErrArg("str.IsNullOrEmpty(Req.UniqueName)"); //TODO 優化異常處理 錯誤碼, 前端多語言
				}
				PoUser = await SelectUserByUniqueName(Req.UniqueName,Ct);
			}else if(Req.UserIdentityMode == ReqLogin.EUserIdentityMode.Email){
				if(str.IsNullOrEmpty(Req.Email)){
					throw new ErrArg("str.IsNullOrEmpty(Req.Email)"); //TODO 優化異常處理 錯誤碼, 前端多語言
				}
				PoUser = await SelectUserByEmail(Req.Email,Ct);
			}
			if(PoUser == null){
				throw new ErrBase("User not exsists");
			}

			var PoPassword = await SelectPasswordById(PoUser.Id, Ct);
			if(PoPassword is null){
				throw new ErrBase("Password not exsists");
			}
			if(! await ToolArgon.Inst.VerifyPasswordAsy(Req.Password??"", PoPassword.Text, Ct) ){
				throw new ErrBase("Password not correct");
			};

			var accessToken = GeneAccessToken(PoUser.Id.ToString());
			//TODO
			await Cache.SetStringAsync($"user:token:{PoUser.Id}", accessToken, new DistributedCacheEntryOptions {
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
			}, Ct);

			var RespLogin = new RespLogin{
				AccessToken = accessToken
				,PoUser = PoUser
				,UserIdStr = PoUser.Id.ToString()
			};
			return RespLogin;
		};
	}

	[Impl]
	public async Task<nil> AddUser(ReqAddUser ReqAddUser ,CT Ct){
		return await TxnWrapper.Wrap(FnAddUser, ReqAddUser, Ct);
	}

	[Impl]
	public async Task<RespLogin> Login(ReqLogin ReqLogin ,CT Ct){
		return await TxnWrapper.Wrap(FnLogin, ReqLogin, Ct);
	}

	[Impl]
	public async Task<nil> Logout(ReqLogout ReqLogout, CT Ct){
		return NIL;
	}

}
