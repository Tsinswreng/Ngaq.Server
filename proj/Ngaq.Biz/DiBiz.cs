using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ngaq.Biz.Db;
using Tsinswreng.CsSqlHelper;
using Tsinswreng.CsSqlHelper.EFCore;
using Tsinswreng.CsSqlHelper.PostgreSql;
using Ngaq.Biz.Db.User;
using Ngaq.Biz.Svc;
using Ngaq.Core.Model.Sys.Po.Password;
using Ngaq.Core.Model.Sys.Po.User;

namespace Ngaq.Biz;

public static class DiBiz{
	static IServiceCollection AddRepoScoped<TEntity, TId>(
		this IServiceCollection z
	){
		z.AddScoped<IRepo<TEntity, TId>, EfRepo<TEntity, TId>>();
		return z;
	}
	public static IServiceCollection SetUpBiz(this IServiceCollection z){
		z.AddDbContext<ServerDbCtx>();
		z.AddTransient<ITxnRunner, EfTxnRunner>();
		z.AddTransient<DbFnCtxMkr>();
		z.AddScoped<I_GetTxnAsy, PostgreSqlCmdMkr>();
		z.AddScoped<IDbConnection>((s)=>{
			var DbCtx = s.GetRequiredService<ServerDbCtx>();
			var R = DbCtx.Database.GetDbConnection();
			R.Open();
			return R;
		});


		z.AddDbContext<ServerDbCtx>();
		z.AddScoped<DbFnCtxMkr>();
		z.AddScoped<ITxnRunner, EfTxnRunner>();
		z.AddRepoScoped<PoUser, IdUser>();
		z.AddRepoScoped<PoPassword, IdPassword>();
		z.AddScoped<DaoUser>();
		z.AddScoped<SvcUser>();


		return z;
	}
}
