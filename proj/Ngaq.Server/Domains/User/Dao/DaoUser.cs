namespace Ngaq.Server.Db.User;

using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Models.Sys.Po.Password;
using Ngaq.Backend.Db.TswG;
using Tsinswreng.CsSql;

public partial class DaoUser(
	ISqlCmdMkr SqlCmdMkr
	,ITblMgr TblMgr
	//,IAppRepo<PoWordLearn, IdWordLearn> RepoLearn
){


	/// <summary>
	/// 按唯一名稱查詢用戶。
	/// </summary>
	public async Task<PoUser?> SelectByUniqName(
		IDbFnCtx Ctx
		,str UniqName
		,CT Ct
	){
var T = TblMgr.GetTbl<PoUser>();
var Sql = T.SqlSplicer().Select("*").From()
.Where1().And(T.SqlIsNonDel()).AndEq(x=>x.UniqName, out var PUniqName).ToSqlStr();

var Cmd = await SqlCmdMkr.Prepare(Ctx, Sql, Ct);
Ctx?.AddToDispose(Cmd);
		var Args = ArgDict.Mk(T).AddT(PUniqName, UniqName);
		var R = await Cmd.WithCtx(Ctx).Args(Args).FirstOrDefault<PoUser>(T, Ct);
		return R;
	}

	/// <summary>
	/// 按郵箱查詢用戶。
	/// </summary>
	public async Task<PoUser?> SelectByEmail(
		IDbFnCtx Ctx
		,str Email
		,CT Ct
	){
var T = TblMgr.GetTbl<PoUser>();
// var N = new PoUser.N();
// var PEmail = T.Prm(N.Email);
// var Sql =
// $"""
// SELECT * FROM {T.Qt(T.DbTblName)}
// WHERE 1=1
// AND {T.Eq(PEmail)}
// """;
var Sql = T.SqlSplicer().Select("*").From()
.Where1().AndEq(x=>x.Email, out var PEmail).ToSqlStr();

var Cmd = await SqlCmdMkr.Prepare(Ctx, Sql, Ct);
Ctx?.AddToDispose(Cmd);
		var Args = ArgDict.Mk(T).AddT(PEmail, Email);
		var R = await Cmd.WithCtx(Ctx).Args(Args).FirstOrDefault<PoUser>(T, Ct);
		return R;
	}

	/// <summary>
	/// 按用戶Id查詢密碼記錄。
	/// </summary>
	public async Task<PoPassword?> SlctPasswordByUserId(
		IDbFnCtx Ctx
		,IdUser UserId
		,CT Ct
	){
var T = TblMgr.GetTbl<PoPassword>();
var Sql = T.SqlSplicer().Select("*").From()
.Where1().And(T.SqlIsNonDel()).AndEq(x=>x.UserId, out var PUserId).ToSqlStr();
// var PUserId = T.Prm(nameof(PoPassword.UserId));
// var Sql =
// $"""
// SELECT * FROM {T.Qt(T.DbTblName)}
// WHERE 1=1
// AND {T.SqlIsNonDel()}
// AND {T.Eq(PUserId)}
//""";
var SqlCmd = await SqlCmdMkr.Prepare(Ctx, Sql, Ct); Ctx?.AddToDispose(SqlCmd);
		var Args = ArgDict.Mk(T).AddT(PUserId, UserId);
		var R = await SqlCmd.WithCtx(Ctx).Args(Args).FirstOrDefault<PoPassword>(T, Ct);
		return R;
	}
}


