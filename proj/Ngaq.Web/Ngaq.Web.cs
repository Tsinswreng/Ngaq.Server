using System.Text.Json.Serialization;
using Ngaq.Biz.Infra.Cfg;
using Ngaq.Biz;
using Ngaq.Web;
using Tsinswreng.CsCfg;
using CfgItems = Ngaq.Biz.Infra.Cfg.ItemsServerCfg;
using Ngaq.Core.Infra;
using Ngaq.Core.Tools;
using Tsinswreng.CsTools;
using Ngaq.Web.Midware;


var app = NgaqWeb.InitApp(args);
app.Run();


public class NgaqWeb{
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
	var Port = ItemsServerCfg.Port.GetFrom(ServerCfg.Inst);
	options.ListenLocalhost(Port);  // 监听本地端口
	// 或者用 options.ListenAnyIP(5001); 监听所有IP地址
});

builder.Services.ConfigureHttpJsonOptions(opt =>
{
	opt.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonCtx.Default);
	opt.SerializerOptions.PropertyNamingPolicy = null;
	opt.SerializerOptions.WriteIndented = true;
	opt.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

//fucking Asp.net don't use my already defined converters in AppJsonCtx through [JsonSourceGenerationOptions(Converters=[...])]
	opt.SerializerOptions.Converters.AddRange(
		AppJsonCtx.JsonConverters
	);
	//我已經給AppJsonCtx配過一堆Converters了 能不能在這裏直接調用 而不是讓我再手上配一次
	//opt.SerializerOptions.Converters.Add();
	//opt.SerializerOptions.Converters.Add(new CustomJsonConvtrFctry());
});

//builder.Services.AddAuthentication().AddBearerToken();

builder.Services
	.SetupEntry(Cfg)
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
app.UseMiddleware<TokenValidationMidware>();
app.UseMiddleware<EdgeDebounceMidware>();

app.MapGet("/", async()=>{
	return new Tempus().ToString();
});


var BaseRoute = app.MapGroup("/"); //RouteGroupBuilder

var Svc = app.Services;
appRouterIniter.InitRouters(Svc, BaseRoute);
return app;
	}
}

