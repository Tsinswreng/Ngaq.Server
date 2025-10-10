namespace Ngaq.Biz;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ngaq.Biz.Db;
using Tsinswreng.CsSqlHelper;
using Tsinswreng.CsSqlHelper.EFCore;
using Tsinswreng.CsSqlHelper.Postgres;
using Ngaq.Biz.Db.User;
using Ngaq.Biz.Svc;
using Ngaq.Core.Model.Sys.Po.Password;
using Ngaq.Core.Model.Sys.Po.User;
using Ngaq.Local.Db;
using Ngaq.Biz.Infra.Cfg;
using Ngaq.Biz.Db.TswG;
using Ngaq.Core.Models.Sys.Po.User;
using Npgsql;
using Ngaq.Core.Models.Sys.Po.Password;

public static class DiBiz{
	static IServiceCollection SetupEfCore(this IServiceCollection z){
		z.AddDbContext<ServerDbCtx>();
		z.AddScoped<DbContext>(provider => provider.GetRequiredService<ServerDbCtx>());//EfRepo要用
		z.AddTransient<ITxnRunner, EfTxnRunner>();
		z.AddDbContext<ServerDbCtx>();
		return z;
	}
	static IServiceCollection SetupTswGSqlEf(this IServiceCollection z){
		z.AddTransient<DbFnCtxMkr<DbFnCtx>>();
		z.AddScoped<I_GetTxnAsy, PostgresCmdMkr>();
		z.AddScoped<IDbFnCtxMkr<DbFnCtx>, DbFnCtxMkr<DbFnCtx>>();
		z.AddScoped<ITxnRunner, EfTxnRunner>();
		z.AddScoped<TxnWrapper<DbFnCtx>>();
		z.AddScoped<IDbConnection>((s)=>{
			var DbCtx = s.GetRequiredService<ServerDbCtx>();
			var R = DbCtx.Database.GetDbConnection();
			R.Open();
			return R;
		});
		return z;
	}

	static IServiceCollection SetupTswGSqlAdo(this IServiceCollection z){
		//事務執行器
		z.AddTransient<ITxnRunner, AdoTxnRunner>();
		//
		z.AddTransient<DbFnCtxMkr<DbFnCtx>>();

		z.AddScoped<I_GetTxnAsy, PostgresCmdMkr>();
		z.AddScoped<IDbFnCtxMkr<DbFnCtx>, DbFnCtxMkr<DbFnCtx>>();
		//事務函數包裝器
		z.AddScoped<TxnWrapper<DbFnCtx>>();
		//z.AddScoped<IDbConnection>();
		z.AddSingleton<NpgsqlDataSource>(ServerDb.Inst.DataSource);
		z.AddSingleton<IDbConnPool, PostgresConnPool>();
		return z;
	}

	static IServiceCollection AddRepoScoped<TEntity, TId>(
		this IServiceCollection z
	)where TEntity:class
	{
		z.AddScoped<IRepo<TEntity, TId>, EfRepo<TEntity, TId>>();
		return z;
	}
	public static IServiceCollection SetupBiz(this IServiceCollection z){
		z.SetupTswGSqlAdo();
		z.AddRepoScoped<PoUser, IdUser>();
		z.AddRepoScoped<PoPassword, IdPassword>();
		z.AddScoped<DaoUser>();
		z.AddScoped<SvcUser>();

		// 配置 Redis 连接
		var Cfg = ServerCfg.Inst;
		// var configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
		// // 可选：根据需要配置其他选项，例如：
		// configurationOptions.AbortOnConnectFail = false; // 防止连接失败时立即中止
		// configurationOptions.Ssl = true; // 如果需要 SSL 连接
		// z.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(configurationOptions));

		return z;
	}
}
