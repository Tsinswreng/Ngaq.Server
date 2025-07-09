using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ngaq.Biz.Db;
using Tsinswreng.CsSqlHelper;
using Tsinswreng.CsSqlHelper.EFCore;
using Tsinswreng.CsSqlHelper.PostgreSql;

namespace Ngaq.Biz;

public class DiBiz{
	public static IServiceCollection SetUpBiz(IServiceCollection z){
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
		return z;
	}
}
