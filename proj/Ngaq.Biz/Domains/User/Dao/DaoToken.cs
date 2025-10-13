namespace Ngaq.Biz.Domains.User.Dao;

using Ngaq.Core.Model.Sys.Po.RefreshToken;
using Ngaq.Core.Models.Sys.Po.RefreshToken;
using Ngaq.Local.Db.TswG;
using Tsinswreng.CsPage;
using Tsinswreng.CsSqlHelper;

public class DaoToken(
	ISqlCmdMkr SqlCmdMkr
	,ITblMgr TblMgr
	,IAppRepo<PoRefreshToken, IdRefreshToken> RepoToken
){


	public async Task<Func<
		IEnumerable<PoRefreshToken>
		,CT, Task<nil>
	>> FnAddTokens(IDbFnCtx? Ctx, CT Ct){
		var Add = await RepoToken.FnInsertMany(Ctx, Ct);
		return async(Tokens,Ct)=>{
			await Add(Tokens, Ct);
			return NIL;
		};
	}

	// public async Task<Func<
	// 	IdUser
	// 	,CT, Task<IPageAsyE<PoRefreshToken>>
	// >>

}
