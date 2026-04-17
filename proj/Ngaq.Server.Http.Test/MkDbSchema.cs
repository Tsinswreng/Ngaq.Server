
using Microsoft.Extensions.DependencyInjection;
using Ngaq.Server.Db.TswG;
using Ngaq.Server.Db.TswG.Migrations;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Shared.Kv.Svc;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Local.Db.TswG;
using Ngaq.Local.Word.Dao;
using Tsinswreng.CsSql;
using Ngaq.Server.Http;
namespace Ngaq.Server.Http.Test;

public class MkDbSchema{
	public static async Task InitDb(str[] args){
		var app = NgaqWeb.InitApp(args);
		var fullInit = app.Services.GetRequiredService<FullInit>();
		await fullInit.Up(default);
		System.Console.WriteLine("done");
	}

	public static async Task MigrateDb(str[] args){
		var app = NgaqWeb.InitApp(args);
		var migrator = app.Services.GetRequiredService<MigrationRunner>();
		await migrator.Up(default);
		System.Console.WriteLine("migrate done");
	}
}
