using System.Text.Json.Serialization;
using Ngaq.Biz.Infra.Cfg;
using Ngaq.Biz;
using Ngaq.Web;
using Tsinswreng.CsCfg;
using CfgItems = Ngaq.Biz.Infra.Cfg.ServerCfgItems;
using Ngaq.Core.Infra;


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
	var Port = ServerCfgItems.Port.GetFrom(ServerCfg.Inst);
	options.ListenLocalhost(Port);  // 监听本地端口
	// 或者用 options.ListenAnyIP(5001); 监听所有IP地址
});

builder.Services.ConfigureHttpJsonOptions(opt =>
{
	opt.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonCtx.Default);
	opt.SerializerOptions.PropertyNamingPolicy = null;
	opt.SerializerOptions.WriteIndented = true;
	opt.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services
	.Setup(Cfg)
;


//AppRouterIniter.Inst.RegisterCtrlr();
var AppRouterIniter = new AppRouterIniter(builder.Services);
var app = builder.Build();

//cors
app.UseCors();

app.MapGet("/", async()=>{
	return new Tempus().ToString();
});


var BaseRoute = app.MapGroup("/"); //RouteGroupBuilder

var Svc = app.Services;
AppRouterIniter.Init(Svc, BaseRoute);
return app;
	}
}

