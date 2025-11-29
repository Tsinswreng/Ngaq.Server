namespace Ngaq.Biz.Domains.User.Dao;

using Ngaq.Core.Shared.User.Models.Po.RefreshToken;
using Ngaq.Core.Model.Sys.Po.RefreshToken;
using Ngaq.Local.Db.TswG;
using Tsinswreng.CsPage;
using Tsinswreng.CsSqlHelper;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.User.Models.Po.Device;
using Tsinswreng.CsCore;

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
			var R = await Ctx.RunCmd(Cmd, Arg).FirstOrDefault<PoRefreshToken>(T, Ct);
			return R;
		};
	}

[Doc("""
據UserId和ClientId取得所有有效的RefreshToken
""")]
	public async Task<Func<
		IdUser, IdClient
		,CT, Task<IAsyncEnumerable<PoRefreshToken>>
	>> FnSlctValidTokens(IDbFnCtx Ctx, CT Ct){
var T = TblMgr.GetTbl<PoRefreshToken>();
var PUserId = T.Prm(nameof(PoRefreshToken.UserId));var PClientId = T.Prm(nameof(PoRefreshToken.ClientId));
var Sql = $"""
SELECT * FROM {T.Qt(T.DbTblName)}
WHERE 1=1
AND {T.SqlIsNonDel()}
AND {T.Eq(PUserId)}
AND {T.Eq(PClientId)}
ORDER BY {T.Fld(nameof(PoRefreshToken.DbCreatedAt))} DESC
""";
		var Cmd = await Ctx.PrepareToDispose(SqlCmdMkr, Sql, Ct);
		return async(UserId, ClientId, Ct)=>{
			var Arg = ArgDict.Mk(T).AddT(PUserId, UserId).AddT(PClientId, ClientId);
			var RawAsyE = Ctx.RunCmd(Cmd, Arg).IterAsyE(Ct);
			return RawAsyE.Select(x=>T.DbDictToEntity<PoRefreshToken>(x));
		};
	}





	// public async Task<Func<
	// 	IdUser
	// 	,CT, Task<IPageAsyE<PoRefreshToken>>
	// >>

}
