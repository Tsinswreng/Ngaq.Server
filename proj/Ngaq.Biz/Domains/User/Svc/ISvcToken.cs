namespace Ngaq.Biz.Domains.User.Svc;

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



}

