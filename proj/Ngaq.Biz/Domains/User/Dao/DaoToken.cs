namespace Ngaq.Biz.Domains.User.Dao;

using Ngaq.Core.Shared.User.Models.Po.RefreshToken;
using Ngaq.Core.Model.Sys.Po.RefreshToken;
using Ngaq.Local.Db.TswG;
using Tsinswreng.CsPage;
using Tsinswreng.CsSqlHelper;

public class DaoToken(
	ISqlCmdMkr SqlCmdMkr
	,ITblMgr TblMgr
){

	public async Task<Func<
		u8[] // TokenValue
		,CT, Task<PoRefreshToken?>
	>> FnSlctByTokenValue(IDbFnCtx Ctx, CT Ct){
		var T = TblMgr.GetTbl<PoRefreshToken>();
		var PTokenValue = T.Prm(nameof(PoRefreshToken.TokenValue));
var Sql = $"""
SELECT * FROM {T.Qt(T.DbTblName)}
WHERE 1=1
AND {T.SqlIsNonDel()}
AND {T.Eq(PTokenValue)}
""";
		var Cmd = await Ctx.PrepareToDispose(SqlCmdMkr, Sql, Ct);
		return async (TokenValue, Ct)=>{
			var Arg = ArgDict.Mk(T).AddT(PTokenValue, TokenValue);
			var R = await Ctx.Attach(Cmd, Arg).FirstOrDefault<PoRefreshToken>(T, Ct);
			return R;
		};
	}

	// public async Task<Func<
	// 	IdUser
	// 	,CT, Task<IPageAsyE<PoRefreshToken>>
	// >>

}
