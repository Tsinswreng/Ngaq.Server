using Microsoft.Extensions.DependencyInjection;
using Ngaq.Server.Db.TswG;
using Ngaq.Server.Db.TswG.Migrations;
using Ngaq.Server.Infra.Cfg;
using Npgsql;
using Tsinswreng.CsCfg;

namespace Ngaq.Server.Test;

public static class ServerTestBootstrap{
	public static async Task EnsureServerDbReady(IServiceProvider sp, CT ct){
		await EnsureDatabaseExists(ct);

		var fullInit = sp.GetRequiredService<FullInit>();
		var hasSchemaHistory = await HasSchemaHistoryTable(ct);
		if(!hasSchemaHistory){
			await fullInit.Up(ct);
		}
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

		await using var existsCmd = conn.CreateCommand();
		existsCmd.CommandText = "SELECT EXISTS(SELECT 1 FROM pg_database WHERE datname = @dbName)";
		existsCmd.Parameters.AddWithValue("@dbName", dbName ?? "");
		var exists = (bool)(await existsCmd.ExecuteScalarAsync(ct) ?? false);
		if(!exists){
			await using var createCmd = conn.CreateCommand();
			createCmd.CommandText = $"CREATE DATABASE \"{dbName}\"";
			await createCmd.ExecuteNonQueryAsync(ct);
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
