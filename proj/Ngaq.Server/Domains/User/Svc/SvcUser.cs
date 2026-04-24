namespace Ngaq.Server.Domains.User.Svc;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Ngaq.Server.Db.User;
using Ngaq.Server.Tools;
using Ngaq.Core.Shared.User.Models.Req;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Model.Sys.Po.Password;
using Ngaq.Core.Models.Sys.Po.Password;
using Ngaq.Core.Models.Sys.Req;
using Ngaq.Core.Tools;
using Ngaq.Backend.Db.TswG;
using Tsinswreng.CsCore;
using Tsinswreng.CsSql;
using Ngaq.Core.Shared.Base.Models.Req;
using Ngaq.Core.Shared.User.Models.Resp;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.User.Svc;
using Ngaq.Core.Shared.User.UserCtx;
using Tsinswreng.CsErr;
using Ngaq.Core.Tools.Json;

public partial class SvcUser(
	DaoUser DaoUser
	, IRepo<PoUser, IdUser> RepoUser
	, IRepo<PoPassword, IdPassword> RepoPassword
	,ISqlCmdMkr SqlCmdMkr
	// ,DbFnCtxMkr DbFnCtxMkr
	// ,ITxnRunner TxnRunner
	,IDistributedCache Cache
	,ILogger<SvcUser> Log
	,ISvcToken SvcToken
	,IJsonSerializer JsonS
)
	:ISvcUser
{

	
	///TODO 檢查用戶存在否

	/// <summary>
	/// 在事務內創建用戶與密碼記錄。
	/// </summary>
	protected async Task<nil> AddUserInTxn(
		IDbFnCtx Ctx
		,IUserCtx UsrCtx
		,ReqAddUser Req
		,CT Ct
	){
		Req.Validate();
		var Id = new IdUser();
		var PoUser = new PoUser{
			Id = Id
			,UniqName = Req.UniqName??Id.ToString()
			,Email = Req.Email
		};
		var PasswordHash = await ToolArgon.Inst.HashPasswordAsy(Req.Password, Ct);
		var Password = new PoPassword{
			Id = new()
			,UserId = PoUser.Id
			,Algo = PoPassword.EAlgo.Argon2id
			,Text = PasswordHash
		};
		await RepoUser.BatAdd(Ctx, OneAsyE(PoUser), Ct);
		await RepoPassword.BatAdd(Ctx, OneAsyE(Password), Ct);
		try{
			await Cache.SetStringAsync($"user:register:{PoUser.Id}", JsonS.Stringify(PoUser), new DistributedCacheEntryOptions {
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
			}, Ct);
		}catch(Exception e){
			Log.LogError(e, "Failed to cache user registration info.");
		}
		return NIL;
	}

	/// <summary>
	/// 在事務內校驗密碼並簽發訪問/刷新令牌。
	/// </summary>
	protected async Task<RespLogin> LoginInTxn(
		IDbFnCtx Ctx
		,IUserCtx User
		,ReqLogin Req
		,CT Ct
	){
		PoUser? PoUser = null;
		if(Req.UserIdentityMode == ReqLogin.EUserIdentityMode.UniqName){
			if(str.IsNullOrEmpty(Req.UniqName)){
				throw KeysErr.Common.ArgErr.ToErr([nameof(Req.UniqName)]);
			}
			PoUser = await DaoUser.SelectByUniqName(Ctx, Req.UniqName, Ct);
		}else if(Req.UserIdentityMode == ReqLogin.EUserIdentityMode.Email){
			if(str.IsNullOrEmpty(Req.Email)){
				throw KeysErr.Common.ArgErr.ToErr([nameof(Req.Email)]);
			}
			PoUser = await DaoUser.SelectByEmail(Ctx, Req.Email, Ct);
		}
		if(PoUser == null){
			throw KeysErr.User.PasswordNotMatch.ToErr();
		}
		var PoPassword = await DaoUser.SlctPasswordByUserId(Ctx, PoUser.Id, Ct);
		if(PoPassword is null){
			throw KeysErr.User.PasswordNotMatch.ToErr();
		}
		if(! await ToolArgon.Inst.VerifyPasswordAsy(Req.Password??"", PoPassword.Text, Ct) ){
			throw KeysErr.User.PasswordNotMatch.ToErr();
		}
		var UserCtx = User.AsServerUserCtx();
		UserCtx.UserId = PoUser.Id;

		var AToken = SvcToken.GenAccessToken(new ReqGenAccessToken{
			UserId = PoUser.Id.ToString()
		});
		var RTokenResp = await SvcToken.GenEtStoreRefreshToken(Ctx, UserCtx, Ct);

		var RespLogin = new RespLogin{
			AccessToken = AToken.AccessToken
			,AccessTokenExpireAt = AToken.ExpireAt
			,UserId = PoUser.Id.ToString()
			,RefreshToken = RTokenResp.RefreshToken
			,RefreshTokenExpireAt = RTokenResp.ExpireAt
		};
		return RespLogin;
	}

	/// <summary>
	/// 在事務內撤銷當前客戶端的有效刷新令牌。
	/// </summary>
	protected async Task<nil> LogoutInTxn(
		IDbFnCtx Ctx
		,IUserCtx User
		,CT Ct
	){
		await SvcToken.RevokeTokensForLogout(Ctx, User, Ct);
		return NIL;
	}

	protected static async IAsyncEnumerable<T> OneAsyE<T>(T Item){
		yield return Item;
	}

	[Impl]
	public async Task<nil> AddUser(IUserCtx User, ReqAddUser ReqAddUser ,CT Ct){
		return await SqlCmdMkr.RunInTxn(Ct, async(Ctx)=>{
			return await AddUserInTxn(Ctx, User, ReqAddUser, Ct);
		});
	}

	[Impl]
	public async Task<RespLogin> Login(IUserCtx User, ReqLogin ReqLogin ,CT Ct){
		return await SqlCmdMkr.RunInTxn(Ct, async(Ctx)=>{
			return await LoginInTxn(Ctx, User, ReqLogin, Ct);
		});
	}

	[Impl]
	public async Task<nil> Logout(IUserCtx User, ReqLogout ReqLogout, CT Ct){
		return await SqlCmdMkr.RunInTxn(Ct, async(Ctx)=>{
			return await LogoutInTxn(Ctx, User, Ct);
		});
	}
}

