using System.Text.Json.Serialization;
using Ngaq.Server.Infra.Cfg;
using Ngaq.Server;
using Ngaq.Server.Http;
using Tsinswreng.CsCfg;
using CfgItems = Ngaq.Server.Infra.Cfg.KeysServerCfg;
using Ngaq.Core.Infra;
using Ngaq.Core.Tools;
using Tsinswreng.CsTools;
using Ngaq.Server.Http.Midware;
using Tsinswreng.CsCore;


namespace Ngaq.Server.Http;


public class NgaqWeb{
	public static void Main(string [] args){
		var app = NgaqWeb.InitApp(args);
		app.Run();
	}
	
	public static WebApplication InitApp(str[] args){
var Cfg = ServerCfg.Inst;
System.Console.WriteLine(
	"pwd: "+Directory.GetCurrentDirectory()
);
Cfg.LoadFromArgs(args);

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
	var Port = KeysServerCfg.Port.GetFrom(ServerCfg.Inst);
	options.ListenLocalhost(Port);  // 监听本地端口
	// 或者用 options.ListenAnyIP(5001); 监听所有IP地址
});

builder.Services.ConfigureHttpJsonOptions(opt =>
{
	opt.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonCtx.Inst);
	opt.SerializerOptions.PropertyNamingPolicy = null;
	opt.SerializerOptions.WriteIndented = true;
	opt.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
	opt.SerializerOptions.Converters.AddRange(
		AppJsonCtx.JsonConverters
	);
	opt.SerializerOptions.Converters.Add(
		new JsonStringEnumConverter()
	);
	
});

//builder.Services.AddAuthentication().AddBearerToken();

builder.Services
	.SetupHttpServer(Cfg)
;

builder.Services.AddExceptionHandler<GlobalErrHandler>();
builder.Services.AddProblemDetails(); // ✅ 必须加这个

//AppRouterIniter.Inst.RegisterCtrlr();
var appRouterIniter = AppRouterIniter.Inst;
appRouterIniter.Init(builder.Services);

var app = builder.Build();
app.UseExceptionHandler();

//cors
app.UseCors();

// 托管前端静态资源：优先返回 index.html，并开放 wwwroot 下文件访问。
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseMiddleware<TokenValidationMidware>();
app.UseMiddleware<EdgeDebounceMidware>();


var BaseRoute = app.MapGroup("/"); //RouteGroupBuilder

var Svc = app.Services;
appRouterIniter.InitRouters(Svc, BaseRoute);

// 非 API 路由回退到前端入口，支持前端路由直达。
app.MapFallbackToFile("/index.html");

return app;
	}
}

