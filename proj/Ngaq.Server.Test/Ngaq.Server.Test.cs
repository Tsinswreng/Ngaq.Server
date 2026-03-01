using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Ngaq.Biz.Db.TswG;
using Ngaq.Biz.Db.TswG.Migrations;
using Ngaq.Core.Model.Po.Word;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Shared.Kv.Svc;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Word.Svc;
using Ngaq.Local.Db.TswG;
using Ngaq.Local.Word.Dao;
using Tsinswreng.CsCore;
using Tsinswreng.CsPage;
using Tsinswreng.CsSqlHelper;
// dotnet run -- E:/_code/CsNgaq/Ngaq.Server/ExternalRsrc/Ngaq.Server.dev.jsonc

Program.args = args;
Program.Init();


//await InitDb(args);
static async Task Test(){
	var txnWrapper = Program.SvcProvdr.GetRequiredService<TxnWrapper>();
	var dao = Program.SvcProvdr.GetRequiredService<DaoWord>();
	await txnWrapper.Wrap(dao.FnTextMultiSelect, default);
}

static async Task TryRepoBat(CT Ct){
	var RepoWord = SvcProvdr.GetRequiredService<IAppRepo<PoWord, IdWord>>();
	var Ctx = new DbFnCtx();
	var fnPageAll = await RepoWord.FnPageAll(Ctx, Ct);
	var all = await fnPageAll(PageQry.SlctI64Max(), Ct);
	var data = all.DataAsyE.OrEmpty();
	var allIds = await data.Select(x=>x.Id).ToListAsync(Ct);
	var sw = Stopwatch.StartNew();
	await RepoWord.BatSlctById(Ctx, allIds, Ct);
	sw.Stop();
	Console.WriteLine($"BatSlctById: {sw.ElapsedMilliseconds}ms");
	var slctOne = await RepoWord.FnSlctOneById(Ctx, Ct);
	sw.Restart();
	foreach(var id in allIds){
		var po = await slctOne(id, Ct);
	}
	sw.Stop();
	Console.WriteLine($"SlctOneById: {sw.ElapsedMilliseconds}ms");
	System.Console.WriteLine("allIds.Count: "+ allIds.Count);
}

//await InitTestData();
await TryRepoBat(default);

partial class Program{
	public static str[] args = [];
	public static WebApplication App = null!;
	public static IServiceProvider SvcProvdr = null!;
	public static void Init(){
		App = NgaqWeb.InitApp(args);
		SvcProvdr = App.Services;
	}
}
