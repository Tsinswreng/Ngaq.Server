using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Ngaq.Biz.Infra.Cfg;
using Ngaq.Biz;
using Ngaq.Web;
using Tsinswreng.CsCfg;
using Ngaq.Web.AspNetTools;


static str GetCfgFilePath(string[] args){
	var CfgFilePath = "";
	if(args.Length > 0){
		CfgFilePath = args[0];
	}else{
#if DEBUG
		CfgFilePath = "Ngaq.Server.dev.json";
#else
		CfgFilePath = "Ngaq.Server.json";
#endif
	}
	return CfgFilePath;
}

try{
	System.Console.WriteLine(
		"pwd: "+Directory.GetCurrentDirectory()
	);
	var CfgPath = GetCfgFilePath(args);
	var CfgText = File.ReadAllText(CfgPath);
	ServerCfg.Inst.FromJson(CfgText);
	//AppCfg.Inst = AppCfgParser.Inst.FromYaml(GetCfgFilePath(args));
}
catch (System.Exception e){
	System.Console.Error.WriteLine("Failed to load config file: "+e);
	System.Console.WriteLine("----");
	System.Console.WriteLine();
}


var builder = WebApplication.CreateSlimBuilder(args);

builder.WebHost.UseKestrel(options =>{
	var Port = ServerCfgItems.Inst.Port.GetFrom(ServerCfg.Inst);
	options.ListenLocalhost(Port);  // 监听本地端口
	// 或者用 options.ListenAnyIP(5001); 监听所有IP地址
});

builder.Services.ConfigureHttpJsonOptions(opt =>
{
	opt.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
	opt.SerializerOptions.PropertyNamingPolicy = null;
	opt.SerializerOptions.WriteIndented = true;
	opt.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});


builder.Services
	.SetUpBiz()
	.SetupWeb()
;
//AppRouterIniter.Inst.RegisterCtrlr();
var AppRouterIniter = new AppRouterIniter(builder.Services);
var app = builder.Build();



var BaseRoute = app.MapGroup("/"); //RouteGroupBuilder
var Svc = app.Services;

AppRouterIniter.Init(Svc, new RouteGroup(BaseRoute));


app.Run();


public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);
[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext{

}
