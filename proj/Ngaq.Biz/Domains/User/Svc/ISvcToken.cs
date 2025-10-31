namespace Ngaq.Biz.Domains.User.Svc;

using Ngaq.Biz.Domains.User.Dto;
using Ngaq.Core.Infra.Core;
using Ngaq.Core.Shared.User.Models.Resp;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Local.Db.TswG;

public interface ISvcToken{
	public Task<Func<
		IUserCtx
		,CT, Task<RespGenRefreshToken>
	>> FnGenEtStoreRefreshToken(IDbFnCtx Ctx, CT Ct);

	public RespGenAccessToken GenAccessToken(
		ReqGenAccessToken Req
	);

	public Task<IAnswer<RespValidateAccessToken>> ValidateAccessTokenAsy(
		ReqValidateAccessToken Req, CT Ct
	);

	public Task<IAnswer<RespRefreshBothToken>> ValidateEtRefreshTheToken(
		IUserCtx User, str RefreshToken, CT Ct
	);



}

