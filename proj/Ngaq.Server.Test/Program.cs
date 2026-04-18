using Microsoft.Extensions.DependencyInjection;
using Ngaq.Core;
using Ngaq.Core.Infra.Cfg;
using Ngaq.Local;
using Ngaq.Local.Di;
using Ngaq.Server;
using Ngaq.Server.Db.TswG;
using Ngaq.Server.Db.TswG.Migrations;
using Ngaq.Server.Http;
using Ngaq.Server.Infra.Cfg;
using Ngaq.Core.Infra.Url;
using Ngaq.Core.Test;
using Ngaq.Local.Test;
using Npgsql;
using Tsinswreng.CsCfg;
using Tsinswreng.CsSql;
using Tsinswreng.CsTools;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Server.Test;

internal class Program{
	static async Task Main(string[] args){
		BaseDirMgr.Inst._BaseDir = Directory.GetCurrentDirectory();

		var localCfgPath = TestCfgPath.GetLocalCfgPath(args);
		var serverCfgPath = TestCfgPath.GetServerCfgPath(args);

		Console.WriteLine($"[Server.Test] Local cfg: {localCfgPath}");
		Console.WriteLine($"[Server.Test] Server cfg: {serverCfgPath}");

		LoadLocalCfg(localCfgPath);
		await RunLocalAndCoreTests();
		await RunServerTests(serverCfgPath);
	}

	static async Task RunLocalAndCoreTests(){
		var svc = new ServiceCollection();
		svc
			.SetupCore()
			.SetupLocal()
			.SetupLocalFrontend();

		var mgr = ServerLocalCoreTestMgr.Inst;
		var sp = mgr.InitSvc(svc, sc=>sc.BuildServiceProvider());

		AppIniter.Inst.Sp = sp;
		await AppIniter.Inst.Init(default);

		ITestExecutor executor = new TreeTestExecutor();
		await executor.RunEtPrint(mgr.TestNode);
	}

	static async Task RunServerTests(string serverCfgPath){
		ServerCfg.Inst.LoadFromArgs([serverCfgPath]);

		var svc = new ServiceCollection();
		svc.AddLogging();
		svc
			.SetupCore()
			.SetupBiz();

		var mgr = ServerTestMgr.Inst;
		var sp = mgr.InitSvc(svc, sc=>sc.BuildServiceProvider());

		await ServerDbTestBootstrap.EnsureDbReady(sp, default);

		ITestExecutor executor = new TreeTestExecutor();
		await executor.RunEtPrint(mgr.TestNode);
	}

	static void LoadLocalCfg(string cfgPath){
		var dualSrcCfg = AppCfg.Inst;

		var roCfg = new JsonFileCfgAccessor();
		dualSrcCfg.RoCfg = roCfg;
		roCfg.FromFile(cfgPath);

		var rwCfgPath = KeysClientCfg.RwCfgPath.GetFrom(dualSrcCfg) ?? "Ngaq.Local.Test.Rw.jsonc";
		ToolFile.EnsureFile(rwCfgPath);
		var rwCfg = new JsonFileCfgAccessor();
		dualSrcCfg.RwCfg = rwCfg;
		rwCfg.FromFile(rwCfgPath);
	}
}

public class ServerLocalCoreTestMgr: DiEtTestMgr{
	public static ServerLocalCoreTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? test){
		test = this.TestNode;
		this.RegisterSubMgr(LocalTestMgr.Inst);
		this.RegisterSubMgr(CoreTestMgr.Inst);
		return test;
	}
}

public static class TestCfgPath{
	public static string GetServerCfgPath(string[] args){
		if(args.Length > 0 && !string.IsNullOrWhiteSpace(args[0])){
			return args[0];
		}
		return "Ngaq.Server.test.jsonc";
	}

	public static string GetLocalCfgPath(string[] args){
		if(args.Length > 1 && !string.IsNullOrWhiteSpace(args[1])){
			return args[1];
		}
		return "Ngaq.Local.test.jsonc";
	}
}

public static class ServerDbTestBootstrap{
	public static async Task EnsureDbReady(IServiceProvider sp, CT ct){
		await EnsureDatabaseExists(ct);

		var fullInit = sp.GetRequiredService<FullInit>();

		var hasSchemaHistory = await HasSchemaHistoryTable(ct);
		if(!hasSchemaHistory){
			await fullInit.Up(ct);
		}
		Console.WriteLine("[Server.Test] Skip MigrationRunner.Up in test bootstrap.");
	}

	static async Task EnsureDatabaseExists(CT ct){
		var cfg = ServerCfg.Inst;
		var server = KeysServerCfg.PgServer.GetFrom(cfg);
		var port = KeysServerCfg.PgPort.GetFrom(cfg);
		var user = KeysServerCfg.PgUserId.GetFrom(cfg);
		var password = KeysServerCfg.PgPassword.GetFrom(cfg);
		var dbName = KeysServerCfg.PgDatabase.GetFrom(cfg);

		var adminConnStr = $"Server={server};Port={port};Database=postgres;User Id={user};Password={password}";
		await using var conn = new NpgsqlConnection(adminConnStr);
		await conn.OpenAsync(ct);

		await using(var existsCmd = conn.CreateCommand()){
			existsCmd.CommandText = "SELECT EXISTS(SELECT 1 FROM pg_database WHERE datname = @dbName)";
			existsCmd.Parameters.AddWithValue("@dbName", dbName ?? "");
			var exists = (bool)(await existsCmd.ExecuteScalarAsync(ct) ?? false);
			if(!exists){
				await using var createCmd = conn.CreateCommand();
				createCmd.CommandText = $"CREATE DATABASE \"{dbName}\"";
				await createCmd.ExecuteNonQueryAsync(ct);
			}
		}
	}

	static async Task<bool> HasSchemaHistoryTable(CT ct){
		await using var conn = ServerDb.Inst.DataSource.CreateConnection();
		await conn.OpenAsync(ct);
		await using var cmd = conn.CreateCommand();
		cmd.CommandText =
			"""
			SELECT EXISTS(
				SELECT 1
				FROM information_schema.tables
				WHERE table_schema = 'public'
				AND table_name = '__TsinswrengSchemaHistory'
			)
			""";
		return (bool)(await cmd.ExecuteScalarAsync(ct) ?? false);
	}
}
