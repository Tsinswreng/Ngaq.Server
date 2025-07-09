using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Ngaq.Biz.Infra.Cfg;
using Ngaq.Biz;
using Ngaq.Web;
using Tsinswreng.CsCfg;


static str GetCfgFilePath(string[] args){
	var CfgFilePath = "";
	if(args.Length > 0){
		CfgFilePath = args[0];
	}else{
#if DEBUG
		CfgFilePath = "Ngaq.dev.json";
#else
		CfgFilePath = "Ngaq.json";
#endif
	}
	return CfgFilePath;
}

try{
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

builder.Services.ConfigureHttpJsonOptions(options =>
{
	options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});


builder.Services.SetUpBiz()
.SetupWeb();
//AppRouterIniter.Inst.RegisterCtrlr();
var AppRouterIniter = new AppRouterIniter(builder.Services);
var app = builder.Build();

#region Example
var sampleTodos = new Todo[] {
	new(1, "Walk the dog"),
	new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
	new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
	new(4, "Clean the bathroom"),
	new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", () => sampleTodos);
todosApi.MapGet("/{id}", (int id) =>
	sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
		? Results.Ok(todo)
		: Results.NotFound());
#endregion Example

var BaseRoute = app.MapGroup("/"); //RouteGroupBuilder
var Svc = app.Services;

AppRouterIniter.Init(Svc, BaseRoute);


app.Run();

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext{

}
