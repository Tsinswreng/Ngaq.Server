namespace Ngaq.Server.Domains.User.Dao;

using Ngaq.Core.Shared.User.Models.Po.RefreshToken;
using Ngaq.Core.Model.Sys.Po.RefreshToken;
using Ngaq.Backend.Db.TswG;
using Tsinswreng.CsPage;
using Tsinswreng.CsSql;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.User.Models.Po.Device;
using Tsinswreng.CsCore;

public class DaoToken(
	ISqlCmdMkr SqlCmdMkr
	,ITblMgr TblMgr
){

	ITable<PoRefreshToken> T{get=>TblMgr.GetTbl<PoRefreshToken>();}


	/// <summary>
	/// 按令牌哈希值查詢一條刷新令牌。
	/// </summary>
	public async Task<PoRefreshToken?> SlctByTokenValue(
		IDbFnCtx Ctx
		,u8[] TokenValue
		,CT Ct
	){
var Sql = T.SqlSplicer().Select("*").From()
.Where1().And(T.SqlIsNonDel()).AndEq(x=>x.TokenValue, out var PTokenValue).ToSqlStr();
var Cmd = await Ctx.PrepareToDispose(SqlCmdMkr, Sql, Ct);
		var Arg = ArgDict.Mk(T).AddT(PTokenValue, TokenValue);
		var R = await Ctx.RunCmd(Cmd, Arg).FirstOrDefault<PoRefreshToken>(T, Ct);
		return R;
	}

[Doc("""
據UserId和ClientId取得所有有效的RefreshToken
""")]
	public async Task<IAsyncEnumerable<PoRefreshToken>> SlctValidTokens(
		IDbFnCtx Ctx
		,IdUser UserId
		,IdClient ClientId
		,CT Ct
	){

var Sql = T.SqlSplicer().Select("*").From()
.Where1().And(T.SqlIsNonDel())
.AndEq(x=>x.UserId, out var PUserId)
.AndEq(x=>x.ClientId, out var PClientId)
.OrderByDesc(x=>x.DbCreatedAt).ToSqlStr();
		var Cmd = await Ctx.PrepareToDispose(SqlCmdMkr, Sql, Ct);
		var Arg = ArgDict.Mk(T).AddT(PUserId, UserId).AddT(PClientId, ClientId);
		var RawAsyE = Ctx.RunCmd(Cmd, Arg).AsyE1d(Ct);
		return RawAsyE.Select(x=>T.DbDictToEntity<PoRefreshToken>(x));
	}

}
