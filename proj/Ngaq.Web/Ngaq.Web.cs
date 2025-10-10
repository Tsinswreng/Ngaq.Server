using System.Text.Json.Serialization;
using Ngaq.Biz.Infra.Cfg;
using Ngaq.Biz;
using Ngaq.Web;
using Tsinswreng.CsCfg;
using Ngaq.Web.AspNetTools;
using CfgItems = Ngaq.Biz.Infra.Cfg.ServerCfgItems;
using System.Runtime.InteropServices;

static str GetCfgFilePath(string[] args){
	var CfgFilePath = "";
	if(args.Length > 0){
		CfgFilePath = args[0];
	}else{
#if DEBUG
		CfgFilePath = "Ngaq.Server.dev.jsonc";
#else
		CfgFilePath = "Ngaq.Server.jsonc";
#endif
	}
	return CfgFilePath;
}

var Cfg = ServerCfg.Inst;
//var CfgItems = ServerCfgItems.Inst;

try{
	System.Console.WriteLine(
		"pwd: "+Directory.GetCurrentDirectory()
	);
	var CfgPath = GetCfgFilePath(args);
	var CfgText = File.ReadAllText(CfgPath);
	Cfg.FromJson(CfgText);
	//AppCfg.Inst = AppCfgParser.Inst.FromYaml(GetCfgFilePath(args));
}
catch (System.Exception e){
	System.Console.Error.WriteLine("Failed to load config file: "+e);
	System.Console.WriteLine("----");
	System.Console.WriteLine();
}



// ... other code ...

// var host = CfgItems.RedisHost.GetFrom(Cfg);
// var port = CfgItems.RedisPort.GetFrom(Cfg);
// string redisConnectionString = CfgItems.RedisHost.GetFrom(Cfg)+":"+CfgItems.RedisPort.GetFrom(Cfg);
// IConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConnectionString);
// IDatabase db = redis.GetDatabase();
// ... 在你的 Minimal API 中使用 db 进行 Redis 操作 ...


var builder = WebApplication.CreateSlimBuilder(args);
// 为所有请求启用 CORS (不推荐用于生产环境):
builder.Services.AddCors(opt=>{
	opt.AddDefaultPolicy(plc=>{
		plc.AllowAnyOrigin()
		.AllowAnyHeader()
		.AllowAnyMethod();
	});
});

builder.WebHost.UseKestrel(options =>{
	var Port = ServerCfgItems.Port.GetFrom(ServerCfg.Inst);
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
	.SetupBiz()
	.SetupWeb()
	.AddStackExchangeRedisCache(opt=>{
		var RedisConnStr = CfgItems.RedisHost.GetFrom(Cfg)+":"+CfgItems.RedisPort.GetFrom(Cfg);
		opt.Configuration = RedisConnStr;
		opt.InstanceName = CfgItems.RedisInstanceName.GetFrom(Cfg);
	})
;


//AppRouterIniter.Inst.RegisterCtrlr();
var AppRouterIniter = new AppRouterIniter(builder.Services);
var app = builder.Build();

//cors
app.UseCors();


var BaseRoute = app.MapGroup("/"); //RouteGroupBuilder
var Svc = app.Services;

AppRouterIniter.Init(Svc, new RouteGroup(BaseRoute));


app.Run();


public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);
[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext{

}

