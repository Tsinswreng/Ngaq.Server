namespace Ngaq.Server;

using Microsoft.Extensions.DependencyInjection;
using Tsinswreng.CsSql;
using Tsinswreng.CsSql.Postgres;
using Ngaq.Server.Db.User;
using Ngaq.Core.Model.Sys.Po.Password;
using Ngaq.Server.Infra.Cfg;
using Ngaq.Server.Db.TswG;
using Npgsql;
using Ngaq.Core.Models.Sys.Po.Password;
using Ngaq.Core.Infra;
using Ngaq.Server.Db.TswG.Migrations;
using Ngaq.Local.Db.TswG;
using Ngaq.Server.Domains.User.Svc;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Server.Domains.User.Dao;
using Ngaq.Core.Shared.User.Models.Po.RefreshToken;
using Ngaq.Core.Model.Sys.Po.RefreshToken;
using Tsinswreng.CsCfg;
using Ngaq.Local.Domains.Word.Svc;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Local.Word.Dao;
using Tsinswreng.CsTools;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Model.Po.Learn_;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Model.Po.Kv;
using StackExchange.Redis;
using Tsinswreng.Srefl;

public static class DiBiz{
	#if false
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
	#endif

	static IServiceCollection SetupTswgSqlAdo(this IServiceCollection z){
		//事務執行器
		z.AddTransient<ITxnRunner, AdoTxnRunner>();

		z.AddTransient<MkrDbFnCtx>();
		z.AddTransient<ISqlCmdMkr, PostgresCmdMkr>();
		z.AddScoped<IMkrTxn, PostgresCmdMkr>();
		z.AddScoped<IMkrDbFnCtx, MkrDbFnCtx>();
		//事務函數包裝器
		z.AddScoped<TxnWrapper>();
		z.AddSingleton<NpgsqlDataSource>(ServerDb.Inst.DataSource);
		//z.AddSingleton<I_GetDbConnAsy, PostgresConnPool>();
		z.AddSingleton<IDbConnMgr>(ServerDb.Inst.DbConnPool);
		z.AddSingleton<ITblMgr>(ServerTblMgr.Inst);

		z.AddRepoScoped<SchemaHistory, i64>();
		// 通用遷移執行邏輯在 CsSql；此處僅聲明 Biz 端遷移清單
		z.AddScoped<IMigrationMgr>(sp=>
			new MigrationMgr(
				TblMgr: sp.GetRequiredService<ITblMgr>()
				,SqlCmdMkr: sp.GetRequiredService<ISqlCmdMkr>()
			)
			.UseServerMigrations()
		);
		z.AddTransient<MigrationRunner>();

		z.AddTransient<FullInit>();

		return z;
	}

	//TODO 改依賴IAppRepo洏非IRepo
	static IServiceCollection AddRepoScoped<TEntity, TId>(
		this IServiceCollection z
	)where TEntity:class, new()
	{
		//z.AddScoped<IRepo<TEntity, TId>, EfRepo<TEntity, TId>>();
		z.AddScoped<IRepo<TEntity, TId>, AppRepo<TEntity, TId>>();
		return z;
	}
	public static IServiceCollection SetupBiz(this IServiceCollection z){
		z.AddSingleton<ICfgAccessor>(ServerCfg.Inst);
		z.SetupTswgSqlAdo();
		z.AddSingleton<IPropAccessorReg>(CoreDictMapper.Inst);
		
		z.AddRepoScoped<PoUser, IdUser>();
		z.AddRepoScoped<PoPassword, IdPassword>();

		z.AddRepoScoped<PoWord, IdWord>();
		z.AddRepoScoped<PoWordLearn, IdWordLearn>();
		z.AddRepoScoped<PoWordProp, IdWordProp>();

		z.AddScoped<DaoUser>();
		z.AddScoped<SvcUser>();
		z.AddRepoScoped<PoRefreshToken, IdRefreshToken>();

		z.AddScoped<DaoToken>();
		z.AddScoped<ISvcToken, SvcToken>();

		z.AddScoped<ISvcWord, SvcWord>();
		z.AddScoped<ISvcParseWordList, SvcParseWordList>();
		z.AddScoped<DaoWord, DaoWord>();

		// 配置 Redis 连接
		var Cfg = ServerCfg.Inst;
		var port = Cfg.Get(KeysServerCfg.Redis.Port);
		var host = Cfg.Get(KeysServerCfg.Redis.Host);
		var password = Cfg.Get(KeysServerCfg.Redis.Password);
		var instanceName = Cfg.Get(KeysServerCfg.Redis.InstanceName);

		z.AddStackExchangeRedisCache(o =>{
			o.Configuration = $"{host}:{port},password={password}";
			o.InstanceName = instanceName;
		});

		var redisConfig = ConfigurationOptions.Parse($"{host}:{port},password={password}");
		var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfig);
		z.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);


		// var configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
		// // 可选：根据需要配置其他选项，例如：
		// configurationOptions.AbortOnConnectFail = false; // 防止连接失败时立即中止
		// configurationOptions.Ssl = true; // 如果需要 SSL 连接
		// z.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(configurationOptions));
		return z;
	}

/*
.AddStackExchangeRedisCache(opt=>{
			var RedisConnStr = CfgItems.Redis.Host.GetFrom(Cfg)+":"+CfgItems.Redis.Port.GetFrom(Cfg);
			opt.Configuration = RedisConnStr;
			opt.InstanceName = CfgItems.Redis.InstanceName.GetFrom(Cfg);
		});
 */
}
