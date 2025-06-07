using Ngaq.Biz.Db;
using Ngaq.Biz.Db.User;
using Ngaq.Biz.Tools;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Model.Sys.Po.Password;
using Ngaq.Core.Model.Sys.Po.User;
using Ngaq.Core.Model.Sys.Req;
using Ngaq.Core.Model.Sys.Resp;
using Ngaq.Core.Tools;
using Ngaq.Local.Db;

namespace Ngaq.Biz.Svc;

public class SvcUser(
	DaoUser DaoUser
	,RepoEf<PoUser, IdUser> RepoUser
	,RepoEf<PoPassword, IdPassword> RepoPassword
){
	public async Task<Func<
		ReqAddUser
		,Task<nil>
	>> FnAddUser(
		IDbFnCtx DbFnCtx
		,CancellationToken ct
	){
		var AddUsers = await RepoUser.FnInsertManyAsy(DbFnCtx, ct);
		var AddPasswords = await RepoPassword.FnInsertManyAsy(DbFnCtx, ct);
		var Ans = async(ReqAddUser ReqAddUser)=>{
			//TODO校驗
			var User = new PoUser{
				Id = new()
				,UniqueName = ReqAddUser.UniqueName
				,Email = ReqAddUser.Email
			};
			var PasswordHash = await ToolArgon.Inst.HashPasswordAsy(ReqAddUser.Password, ct);
			var Password = new PoPassword{
				Id = new()
				,UserId = User.Id
				,Algo = (i64)PoPassword.EAlgo.Argon2id
				,Text = PasswordHash
			};
			await AddUsers([User], ct);
			await AddPasswords([Password], ct);
			return Nil;
		};
		return Ans;
	}

	public async Task<Func<
		ReqLogin
		,Task<RespLogin>
	>> FnLogin(
		IDbFnCtx DbFnCtx
		,CancellationToken ct
	){
		//var SelectUserById = await RepoUser.FnSelectByIdAsy(DbFnCtx, ct);
		var SelectUserByUniqueName = await DaoUser.FnSelectByUniqueName(DbFnCtx, ct);
		var SelectUserByEmail = await DaoUser.FnSelectByEmail(DbFnCtx, ct);
		var SelectPasswordById = await DaoUser.FnSelectPasswordById(DbFnCtx, ct);
		var Fn = async(ReqLogin Req)=>{
			//TODO 校驗Req
			PoUser? PoUser = null;
			if(Req.UserIdentityMode == (i64)ReqLogin.EUserIdentityMode.UniqueName){
				if(str.IsNullOrEmpty(Req.UniqueName)){
					throw new ErrArg("str.IsNullOrEmpty(Req.UniqueName)"); //TODO 優化異常處理 錯誤碼, 前端多語言
				}
				PoUser = await SelectUserByUniqueName(Req.UniqueName,ct);
			}else if(Req.UserIdentityMode == (i64)ReqLogin.EUserIdentityMode.Email){
				if(str.IsNullOrEmpty(Req.Email)){
					throw new ErrArg("str.IsNullOrEmpty(Req.Email)"); //TODO 優化異常處理 錯誤碼, 前端多語言
				}
				PoUser = await SelectUserByEmail(Req.Email,ct);
			}
			if(PoUser == null){
				throw new ErrBase("User not exsists");
			}

			var PoPassword = await SelectPasswordById(PoUser.Id, ct);
			if(!(	await ToolArgon.Inst.VerifyPasswordAsy(Req.Password??"", PoPassword.Text, ct)	)){
				throw new ErrBase("Password not correct");
			};

			var RespLogin = new RespLogin{
				Token = "TODO" //TODO 生成Token
				,PoUser = PoUser
			};
			return RespLogin;
		};
		return Fn;
	}
}
