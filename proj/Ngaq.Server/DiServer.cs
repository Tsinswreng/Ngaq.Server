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
using Ngaq.Backend.Db.TswG;
using Ngaq.Server.Domains.User.Svc;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Server.Domains.User.Dao;
using Ngaq.Core.Shared.User.Models.Po.RefreshToken;
using Ngaq.Core.Model.Sys.Po.RefreshToken;
using Ngaq.Core.Shared.User.Svc;
using Tsinswreng.CsCfg;
using Ngaq.Backend.Domains.Word.Svc;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Backend.Word.Dao;
using Tsinswreng.CsTools;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Model.Po.Learn_;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Model.Po.Kv;
using StackExchange.Redis;
using Tsinswreng.Srefl;
using Ngaq.Core.Shared.Word.Models.Po.UserLang;
using Ngaq.Core.Shared.Dictionary.Models.Po.NormLang;
using Ngaq.Core.Shared.Word.Models.Po.NormLangToUserLang;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Shared.StudyPlan.Models.Po.StudyPlan;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightArg;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngaq.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Tsinswreng.CsCore;
using Ngaq.Backend.Di;

public static class DiBiz{
	
	[Doc("數據庫與ORM基礎設施")]
	static IServiceCollection SetupTswgSqlAdo(this IServiceCollection z){
		//事務執行器
		z.AddTransient<ITxnRunner, AdoTxnRunner>();

		z.AddTransient<MkrDbFnCtx>();
		z.AddTransient<ISqlCmdMkr, PostgresCmdMkr>();
		z.AddScoped<IMkrTxn, PostgresCmdMkr>();
		z.AddScoped<IMkrDbFnCtx, MkrDbFnCtx>();
		//事務函數包裝器
		z.AddScoped<TxnWrapper>();
		z.AddSingleton(ServerDb.Inst.DataSource);
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

	static IServiceCollection AddRepoScoped<TEntity, TId>(
		this IServiceCollection z
	)where TEntity:class, new()
	{
		z.AddScoped<IRepo<TEntity, TId>, AppRepo<TEntity, TId>>();
		return z;
	}
	public static IServiceCollection SetupServer(this IServiceCollection z){
		SetupRepos(z);
		SetupUser(z);
		z.SetupCommonBackend();
		z.AddSingleton<ICfgAccessor>(ServerCfg.Inst);
		z.SetupTswgSqlAdo();
		z.AddSingleton<IPropAccessorReg>(CoreDictMapper.Inst);
		
		z.AddRepoScoped<PoWord, IdWord>();
		z.AddRepoScoped<PoWordLearn, IdWordLearn>();
		z.AddRepoScoped<PoWordProp, IdWordProp>();

		z.AddScoped<ISvcUser>(sp=>sp.GetRequiredService<SvcUser>());
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
	
	static IServiceCollection SetupRepos(this IServiceCollection z){
		z.AddRepoScoped<SchemaHistory, i64>();
		z.AddRepoScoped<PoWord, IdWord>();
		z.AddRepoScoped<PoWordProp, IdWordProp>();
		z.AddRepoScoped<PoWordLearn, IdWordLearn>();
		z.AddRepoScoped<PoUserLang, IdUserLang>();
		z.AddRepoScoped<PoNormLang, IdNormLang>();
		z.AddRepoScoped<PoNormLangToUserLang, IdNormLangToUserLang>();
		z.AddRepoScoped<PoKv, IdKv>();
		z.AddRepoScoped<PoStudyPlan, IdStudyPlan>();
		z.AddRepoScoped<PoWeightArg, IdWeightArg>();
		z.AddRepoScoped<PoWeightCalculator, IdWeightCalculator>();
		z.AddRepoScoped<PoPreFilter, IdPreFilter>();
		//z.AddScoped<IRunInTxn, AdoTxnRunner>();
		return z;
	}
	
	static IServiceCollection SetupUser(this IServiceCollection z){
		z.AddRepoScoped<PoUser, IdUser>();
		z.AddRepoScoped<PoPassword, IdPassword>();
		z.AddTransient<DaoUser>();
		z.AddTransient<SvcUser>();
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
