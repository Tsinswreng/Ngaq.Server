namespace Ngaq.Biz.Db.User;

using Ngaq.Core.Model.Sys.Po.User;
using Ngaq.Core.Models.Sys.Po.Password;
using Ngaq.Core.Models.Sys.Po.User;
using Ngaq.Local.Db.TswG;
using Tsinswreng.CsSqlHelper;

public partial class DaoUser(
	ISqlCmdMkr SqlCmdMkr
	,ITblMgr TblMgr
	//,IAppRepo<PoWordLearn, IdWordLearn> RepoLearn
){


	public async Task<Func<
		str
		,CT
		,Task<PoUser?>
	>> FnSelectByUniqueName(
		IDbFnCtx DbFnCtx
		,CT Ct
	){
		throw new NotImplementedException();
		// var Fn = async(str UniqueName, CT Ct)=>{
		// 	return await DbCtx.User.Where(u => u.UniqueName == UniqueName).FirstOrDefaultAsync(Ct);
		// };
		// return Fn;
	}

	public async Task<Func<
		str
		,CT
		,Task<PoUser?>
	>> FnSelectByEmail(
		IDbFnCtx? Ctx
		,CT Ct
	){
var T = TblMgr.GetTbl<PoUser>(); var N = new PoUser.N();
var PEmail = T.Prm(N.Email);
var Sql =
$"""
SELECT * FROM {T.Qt(T.DbTblName)}
WHERE {T.Fld(PEmail)} = {PEmail}
""";
var Cmd = await SqlCmdMkr.Prepare(Ctx, Sql, Ct);
Ctx?.AddToDispose(Cmd);
		return async(Email, Ct)=>{
			var Args = ArgDict.Mk(T).AddT(PEmail, Email);
			var R = await Cmd.WithCtx(Ctx).Args(Args).FirstOrDefault<PoUser>(T, Ct);
			return R;
		};
	}

	public async Task<Func<
		IdUser
		,CT
		,Task<PoPassword?>
	>> FnSlctPasswordByUserId(
		IDbFnCtx? Ctx
		,CT Ct
	){
var T = TblMgr.GetTbl<PoPassword>(); var PUserId = T.Prm(nameof(PoPassword.UserId));
var Sql =
$"""
SELECT * FROM {T.Qt(T.DbTblName)}
WHERE 1=1
AND {T.Fld(nameof(PoPassword.DelId))} IS NULL
AND {T.Fld(PUserId)} = {PUserId}
"""; var SqlCmd = await SqlCmdMkr.Prepare(Ctx, Sql, Ct); Ctx?.AddToDispose(SqlCmd);
		return async(UserId, Ct)=>{
			var Args = ArgDict.Mk(T).AddT(PUserId, UserId);
			var R = await SqlCmd.WithCtx(Ctx).Args(Args).FirstOrDefault<PoPassword>(T, Ct);
			return R;
		};
	}
}
