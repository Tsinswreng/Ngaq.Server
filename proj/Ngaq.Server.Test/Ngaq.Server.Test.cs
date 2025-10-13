using Microsoft.Extensions.DependencyInjection;
using Ngaq.Biz.Db.TswG;
using Ngaq.Biz.Db.TswG.Migrations;
// dotnet run -- E:/_code/CsNgaq/Ngaq.Server/ExternalRsrc/Ngaq.Server.dev.jsonc
static async Task InitDb(str[] args){
var app = NgaqWeb.InitApp(args);
var fullInit = app.Services.GetRequiredService<FullInit>();
await fullInit.UpAsy(default);
System.Console.WriteLine("done");
}


await InitDb(args);