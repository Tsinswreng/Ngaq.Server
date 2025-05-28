using Microsoft.EntityFrameworkCore;
using Ngaq.Core.Model.Po;
using Ngaq.Core.Model.Sys.Po.Password;
using Ngaq.Core.Model.Sys.Po.User;
using Ngaq.Local.Db;

namespace Ngaq.Biz.Db.User;

public class DaoUser(
	ServerDbCtx DbCtx
)
{

	public async Task<Func<
		str
		,CancellationToken
		,Task<PoUser?>
	>> FnSelectByUniqueName(
		IDbFnCtx DbFnCtx
		,CancellationToken ct
	){
		var Fn = async(str UniqueName, CancellationToken ct)=>{
			return await DbCtx.User.Where(u => u.UniqueName == UniqueName).FirstOrDefaultAsync(ct);
		};
		return Fn;
	}

	public async Task<Func<
		str
		,CancellationToken
		,Task<PoUser?>
	>> FnSelectByEmail(
		IDbFnCtx DbFnCtx
		,CancellationToken ct
	){
		var Fn = async(str Email, CancellationToken ct)=>{
			return await DbCtx.User.Where(u => u.Email == Email).FirstOrDefaultAsync(ct);
		};
		return Fn;
	}

	public async Task<Func<
		IdUser
		,CancellationToken
		,Task<PoPassword>
	>> FnSelectPasswordById(
		IDbFnCtx DbFnCtx
		,CancellationToken ct
	){
		var Fn = async(IdUser UserId, CancellationToken ct)=>{
			return await DbCtx.Password.Where(
				p => p.UserId == UserId
				&& p.Status == (i64)IPoBase.EStatus.Normal
			).FirstOrDefaultAsync(ct);
		};
		return Fn;
	}

}
