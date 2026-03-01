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
using Ngaq.Test;
using Ngaq.Test.Try;
// dotnet run -- E:/_code/CsNgaq/Ngaq.Server/ExternalRsrc/Ngaq.Server.dev.jsonc

Program.args = args;
Program.Init();

await new TryRepoBat{
	SvcProvdr = Program.SvcProvdr
}.Run(default);


//await InitDb(args);
static async Task Test(){
	var txnWrapper = Program.SvcProvdr.GetRequiredService<TxnWrapper>();
	var dao = Program.SvcProvdr.GetRequiredService<DaoWord>();
	await txnWrapper.Wrap(dao.FnTextMultiSelect, default);
}


//await InitTestData();


partial class Program{
	public static str[] args = [];
	public static WebApplication App = null!;
	public static IServiceProvider SvcProvdr = null!;
	public static void Init(){
		App = NgaqWeb.InitApp(args);
		SvcProvdr = App.Services;
	}
}
