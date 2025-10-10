namespace Ngaq.Biz.Db.User;

using Microsoft.EntityFrameworkCore;
using Ngaq.Core.Infra.Core;
using Ngaq.Core.Model.Sys.Po.User;
using Ngaq.Core.Models.Po;
using Ngaq.Core.Models.Sys.Po.Password;
using Ngaq.Core.Models.Sys.Po.Role;
using Ngaq.Core.Models.Sys.Po.User;
using Ngaq.Local.Db;
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
var T = TblMgr.GetTbl<PoUser>();
var NEmail = nameof(PoUser.Email); var PEmail = T.Prm(NEmail);
var Sql =
$"""
SELECT * FROM {T.Qt(T.DbTblName)}
WHERE {T.Fld(NEmail)} = {PEmail}
""";
var Cmd = await SqlCmdMkr.MkCmd(Ctx, Sql, Ct);
Ctx?.AddToDispose(Cmd);
		return async(Email, Ct)=>{
			var Args = ArgDict.Mk().Add(PEmail, T.UpperToRaw(Email));
			var Dicts = await Cmd.WithCtx(Ctx).All(Ct);
			if(Dicts.Count > 1){
				throw new FatalLogicErr("Dicts.Count > 1");
			}
			var Dict = Dicts.SingleOrDefault();
			var R = T.DbDictToEntity<PoUser>(Dict);
			return R;
		};
	}

	public async Task<Func<
		IdUser
		,CT
		,Task<PoPassword>
	>> FnSelectPasswordById(
		IDbFnCtx DbFnCtx
		,CT Ct
	){
		var Fn = async(IdUser UserId, CT Ct)=>{
			return await DbCtx.Password.Where(
				p => p.UserId == UserId
			).FirstOrDefaultAsync(Ct);
		};
		return Fn;
	}
}
