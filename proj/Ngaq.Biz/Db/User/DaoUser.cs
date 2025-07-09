using Microsoft.EntityFrameworkCore;
using Ngaq.Core.Model.Sys.Po.Password;
using Ngaq.Core.Model.Sys.Po.User;
using Ngaq.Core.Models.Po;
using Ngaq.Local.Db;

namespace Ngaq.Biz.Db.User;

public class DaoUser(
	ServerDbCtx DbCtx
){

	public async Task<Func<
		str
		,CT
		,Task<PoUser?>
	>> FnSelectByUniqueName(
		IDbFnCtx DbFnCtx
		,CT Ct
	){
		var Fn = async(str UniqueName, CT ct)=>{
			return await DbCtx.User.Where(u => u.UniqueName == UniqueName).FirstOrDefaultAsync(ct);
		};
		return Fn;
	}

	public async Task<Func<
		str
		,CT
		,Task<PoUser?>
	>> FnSelectByEmail(
		IDbFnCtx DbFnCtx
		,CT Ct
	){
		var Fn = async(str Email, CT Ct)=>{
			return await DbCtx.User.Where(u => u.Email == Email).FirstOrDefaultAsync(Ct);
		};
		return Fn;
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
				&& p.Status == PoStatus.Normal
			).FirstOrDefaultAsync(Ct);
		};
		return Fn;
	}
}
