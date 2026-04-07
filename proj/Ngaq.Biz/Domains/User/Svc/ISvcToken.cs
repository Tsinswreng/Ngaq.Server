namespace Ngaq.Server.Domains.User.Svc;

using Ngaq.Server.Domains.User.Dto;
using Ngaq.Core.Shared.User.Models.Resp;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Local.Db.TswG;
using Tsinswreng.CsErr;
using Tsinswreng.CsSql;

public interface ISvcToken{
	public Task<Func<
		IUserCtx
		,CT, Task<RespGenRefreshToken>
	>> FnGenEtStoreRefreshToken(IDbFnCtx Ctx, CT Ct);

	public RespGenAccessToken GenAccessToken(
		ReqGenAccessToken Req
	);

	public Task<IAnswer<RespValidateAccessToken>> ValidateAccessToken(
		ReqValidateAccessToken Req, CT Ct
	);

	public Task<IAnswer<RespRefreshBothToken>> ValidateEtRefreshTheToken(
		IUserCtx User, str RefreshToken, CT Ct
	);

	public Task<Func<
		IUserCtx
		,CT, Task<nil>
	>> FnRevokeTokensForLogout(IDbFnCtx Ctx, CT Ct);

}

