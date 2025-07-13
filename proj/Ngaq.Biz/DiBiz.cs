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
using Ngaq.Local.Db;
using StackExchange.Redis;
using Ngaq.Biz.Infra.Cfg;
using Tsinswreng.CsCfg;
using Microsoft.Extensions.Caching.Distributed;


namespace Ngaq.Biz;

public static class DiBiz{
	static IServiceCollection AddRepoScoped<TEntity, TId>(
		this IServiceCollection z
	)where TEntity:class
	{
		z.AddScoped<IRepo<TEntity, TId>, EfRepo<TEntity, TId>>();
		return z;
	}
	public static IServiceCollection SetUpBiz(this IServiceCollection z){
		z.AddDbContext<ServerDbCtx>();
		z.AddScoped<DbContext>(provider => provider.GetRequiredService<ServerDbCtx>());//EfRepo要用
		z.AddTransient<ITxnRunner, EfTxnRunner>();
		z.AddTransient<DbFnCtxMkr<DbFnCtx>>();
		z.AddScoped<I_GetTxnAsy, PostgreSqlCmdMkr>();
		z.AddScoped<IDbConnection>((s)=>{
			var DbCtx = s.GetRequiredService<ServerDbCtx>();
			var R = DbCtx.Database.GetDbConnection();
			R.Open();
			return R;
		});

		z.AddDbContext<ServerDbCtx>();
		z.AddScoped<IDbFnCtxMkr<DbFnCtx>, DbFnCtxMkr<DbFnCtx>>();
		z.AddScoped<ITxnRunner, EfTxnRunner>();
		z.AddScoped<TxnWrapper<DbFnCtx>>();
		z.AddRepoScoped<PoUser, IdUser>();
		z.AddRepoScoped<PoPassword, IdPassword>();
		z.AddScoped<DaoUser>();
		z.AddScoped<SvcUser>();

		// 配置 Redis 连接
		var CfgItems = ServerCfgItems.Inst;
		var Cfg = ServerCfg.Inst;
		// var configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
		// // 可选：根据需要配置其他选项，例如：
		// configurationOptions.AbortOnConnectFail = false; // 防止连接失败时立即中止
		// configurationOptions.Ssl = true; // 如果需要 SSL 连接
		// z.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(configurationOptions));

		return z;
	}
}
