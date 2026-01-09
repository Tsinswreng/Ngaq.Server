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

	ITable<PoRefreshToken> T{get=>TblMgr.GetTbl<PoRefreshToken>();}


	public async Task<Func<
		u8[] // TokenValue
		,CT, Task<PoRefreshToken?>
	>> FnSlctByTokenValue(IDbFnCtx Ctx, CT Ct){
var Sql = T.SqlSplicer().Select("*").From()
.WhereT().And(T.SqlIsNonDel()).AndEq(x=>x.TokenValue, out var PTokenValue).ToSqlStr();
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

var Sql = T.SqlSplicer().Select("*").From()
.WhereT().And(T.SqlIsNonDel())
.AndEq(x=>x.UserId, out var PUserId)
.AndEq(x=>x.ClientId, out var PClientId)
.OrderByDesc(x=>x.DbCreatedAt).ToSqlStr();
		var Cmd = await Ctx.PrepareToDispose(SqlCmdMkr, Sql, Ct);
		return async(UserId, ClientId, Ct)=>{
			var Arg = ArgDict.Mk(T).AddT(PUserId, UserId).AddT(PClientId, ClientId);
			var RawAsyE = Ctx.RunCmd(Cmd, Arg).IterAsyE(Ct);
			return RawAsyE.Select(x=>T.DbDictToEntity<PoRefreshToken>(x));
		};
	}

}
