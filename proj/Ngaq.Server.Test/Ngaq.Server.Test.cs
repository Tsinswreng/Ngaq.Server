using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Ngaq.Biz.Db.TswG;
using Ngaq.Biz.Db.TswG.Migrations;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Shared.Kv.Svc;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Word.Svc;
using Ngaq.Local.Db.TswG;
using Ngaq.Local.Word.Dao;
using Tsinswreng.CsSqlHelper;
// dotnet run -- E:/_code/CsNgaq/Ngaq.Server/ExternalRsrc/Ngaq.Server.dev.jsonc

Program.args = args;
Program.Init();

static async Task InitDb(str[] args){
var app = NgaqWeb.InitApp(args);
var fullInit = app.Services.GetRequiredService<FullInit>();
await fullInit.UpAsy(default);
System.Console.WriteLine("done");
}


static async Task InitTestData(){
	var svcWord = Program.Services.GetRequiredService<ISvcWord>();
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

//await InitDb(args);
static async Task Test(){
	var txnWrapper = Program.Services.GetRequiredService<TxnWrapper<DbFnCtx>>();
	var dao = Program.Services.GetRequiredService<DaoSqlWord>();
	await txnWrapper.Wrap(dao.FnTextMultiSelect, default);
}

//await InitTestData();
await Test();

partial class Program{
	public static str[] args = [];
	public static WebApplication App = null!;
	public static IServiceProvider Services = null!;
	public static void Init(){
		App = NgaqWeb.InitApp(args);
		Services = App.Services;
	}
}
