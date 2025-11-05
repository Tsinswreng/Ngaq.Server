namespace Ngaq.Biz.Domains.User.Svc;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Ngaq.Biz.Db.User;
using Ngaq.Biz.Tools;
using Ngaq.Core.Shared.User.Models.Req;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Model.Sys.Po.Password;
using Ngaq.Core.Models.Sys.Po.Password;
using Ngaq.Core.Models.Sys.Req;
using Ngaq.Core.Tools;
using Ngaq.Local.Db.TswG;
using Tsinswreng.CsCore;
using Tsinswreng.CsSqlHelper;
using Ngaq.Core.Shared.Base.Models.Req;
using Ngaq.Core.Shared.User.Models.Resp;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.User.Svc;
using Ngaq.Core.Shared.User.UserCtx;

public partial class SvcUser(
	DaoUser DaoUser
	,IAppRepo<PoUser, IdUser> RepoUser
	,IAppRepo<PoPassword, IdPassword> RepoPassword
	// ,DbFnCtxMkr DbFnCtxMkr
	// ,ITxnRunner TxnRunner
	,TxnWrapper<DbFnCtx> TxnWrapper
	,IDistributedCache Cache
	,ILogger<SvcUser> Log
	,ISvcToken SvcToken
)
	:ISvcUser
{

	/// <summary>
	///TODO 檢查用戶存在否
	/// </summary>
	/// <param name="DbFnCtx"></param>
	/// <param name="Ct"></param>
	/// <returns></returns>
	public async Task<Func<
		IUserCtx
		,ReqAddUser
		,CT
		,Task<nil>
	>> FnAddUser(
		IDbFnCtx? DbFnCtx
		,CT Ct
	){
		var AddUsers = await RepoUser.FnInsertMany(DbFnCtx, Ct);
		var AddPasswords = await RepoPassword.FnInsertMany(DbFnCtx, Ct);
		return async(UsrCtx, Req, Ct)=>{
			Req.Validate();
			var Id = new IdUser();
			var PoUser = new PoUser{
				Id = Id
				,UniqueName = Req.UniqueName??Id.ToString()
				,Email = Req.Email
			};
			var PasswordHash = await ToolArgon.Inst.HashPasswordAsy(Req.Password, Ct);
			var Password = new PoPassword{
				Id = new()
				,UserId = PoUser.Id
				,Algo = PoPassword.EAlgo.Argon2id
				,Text = PasswordHash
			};
			await AddUsers([PoUser], Ct);
			await AddPasswords([Password], Ct);
			//TODO
			try{
				await Cache.SetStringAsync($"user:register:{PoUser.Id}", JSON.stringify(PoUser), new DistributedCacheEntryOptions {
					AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
				}, Ct);
			}catch(Exception e){
				Log.LogError(e, "Failed to cache user registration info.");
			}
			return NIL;
		};
	}


	public async Task<Func<
		IUserCtx
		,ReqLogin
		,CT
		,Task<RespLogin>
	>> FnLogin(
		IDbFnCtx Ctx
		,CT Ct
	){
		//var SelectUserById = await RepoUser.FnSelectByIdAsy(DbFnCtx, ct);
		var SelectUserByUniqueName = await DaoUser.FnSelectByUniqueName(Ctx, Ct);
		var SelectUserByEmail = await DaoUser.FnSelectByEmail(Ctx, Ct);
		var SelectPasswordById = await DaoUser.FnSlctPasswordByUserId(Ctx, Ct);
		var GenEtStoreRToken = await SvcToken.FnGenEtStoreRefreshToken(Ctx,Ct);
		return async(User, Req, Ct)=>{

			//TODO 校驗Req
			PoUser? PoUser = null;
			if(Req.UserIdentityMode == ReqLogin.EUserIdentityMode.UniqueName){
				if(str.IsNullOrEmpty(Req.UniqueName)){
					//throw new ErrArg("str.IsNullOrEmpty(Req.UniqueName)");
					throw ItemsErr.Common.ArgErr.ToErr([nameof(Req.UniqueName)]);
				}
				PoUser = await SelectUserByUniqueName(Req.UniqueName,Ct);
			}else if(Req.UserIdentityMode == ReqLogin.EUserIdentityMode.Email){
				if(str.IsNullOrEmpty(Req.Email)){
					throw ItemsErr.Common.ArgErr.ToErr([nameof(Req.Email)]);
				}
				PoUser = await SelectUserByEmail(Req.Email,Ct);
			}
			if(PoUser == null){
				throw ItemsErr.User.UserNotExist.ToErr();
			}
			var PoPassword = await SelectPasswordById(PoUser.Id, Ct);
			if(PoPassword is null){
				throw new AppErr("Password not exsists");
			}
			if(! await ToolArgon.Inst.VerifyPasswordAsy(Req.Password??"", PoPassword.Text, Ct) ){
				throw ItemsErr.User.PasswordNotMatch.ToErr();
			};
			var UserCtx = User.AsServerUserCtx();
			UserCtx.UserId = PoUser.Id;

			var AToken = SvcToken.GenAccessToken(new ReqGenAccessToken{
				UserId = PoUser.Id.ToString()
			});
			var RTokenResp = await GenEtStoreRToken(UserCtx, Ct);


			//SvcToken.GenRefreshToken()
			//TODO
			// await Cache.SetStringAsync($"user:token:{PoUser.Id}", accessToken, new DistributedCacheEntryOptions {
			// 	AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
			// }, Ct);

			var RespLogin = new RespLogin{
				AccessToken = AToken.AccessToken
				,AccessTokenExpireAt = AToken.ExpireAt
				,PoUser = PoUser
				,UserId = PoUser.Id.ToString()
				,RefreshToken = RTokenResp.RefreshToken
				,RefreshTokenExpireAt = RTokenResp.ExpireAt
			};
			return RespLogin;
		};
	}

	[Impl]
	public async Task<nil> AddUser(IUserCtx User, ReqAddUser ReqAddUser ,CT Ct){
		return await TxnWrapper.Wrap(FnAddUser, User, ReqAddUser, Ct);
	}

	[Impl]
	public async Task<RespLogin> Login(IUserCtx User, ReqLogin ReqLogin ,CT Ct){
		return await TxnWrapper.Wrap(FnLogin, User, ReqLogin, Ct);
	}

	[Impl]
	public async Task<nil> Logout(IUserCtx User, ReqLogout ReqLogout, CT Ct){
		return NIL;
	}

}

