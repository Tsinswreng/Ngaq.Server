namespace Ngaq.Server.Domains.User.Svc;

using Ngaq.Server.Domains.User.Dto;
using Ngaq.Core.Shared.User.Models.Resp;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Backend.Db.TswG;
using Tsinswreng.CsErr;
using Tsinswreng.CsSql;

public interface ISvcToken{
	public Task<RespGenRefreshToken> GenEtStoreRefreshToken(
		IDbFnCtx Ctx
		,IUserCtx User
		,CT Ct
	);

	public RespGenAccessToken GenAccessToken(
		ReqGenAccessToken Req
	);

	public Task<IAnswer<RespValidateAccessToken>> ValidateAccessToken(
		ReqValidateAccessToken Req, CT Ct
	);

	public Task<IAnswer<RespRefreshBothToken>> ValidateEtRefreshTheToken(
		IUserCtx User, str RefreshToken, CT Ct
	);

	public Task<nil> RevokeTokensForLogout(
		IDbFnCtx Ctx
		,IUserCtx User
		,CT Ct
	);

}

