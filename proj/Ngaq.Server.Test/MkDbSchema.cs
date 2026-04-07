using Microsoft.AspNetCore.Builder;
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
namespace Ngaq.Server.Test;

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


	public static async Task InitTestData(){
		var svcWord = Program.SvcProvdr.GetRequiredService<ISvcWord>();
		var list = new List<IJnWord>();
		var user = new UserCtx{
			UserId = new Ngaq.Core.Shared.User.Models.Po.User.IdUser()
		};
		for(var i = 0; i < 10_0000; i++){
			var u = new JnWord();
			u.Word = new PoWord{
				Head = i+""
			};
			list.Add(u);
		}
		await svcWord.AddEtMergeWords(user, list, default);
	}

}
