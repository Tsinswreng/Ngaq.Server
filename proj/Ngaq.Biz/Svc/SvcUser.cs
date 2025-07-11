using Ngaq.Biz.Db;
using Ngaq.Biz.Db.User;
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
)
	: ISvcUser
{
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
				Token = "TODO" //TODO 生成Token
				,PoUser = PoUser
			};
			return RespLogin;
		};
		return Fn;
	}

	[Impl]
	public async Task<nil> AddUser(
		ReqAddUser ReqAddUser
		,CT Ct
	){
		return await TxnWrapper.Wrap(FnAddUser, ReqAddUser, Ct);
	}

	[Impl]
	public async Task<RespLogin> Login(
		ReqLogin ReqLogin
		,CT Ct
	){
		return await TxnWrapper.Wrap(FnLogin, ReqLogin, Ct);
	}

}
