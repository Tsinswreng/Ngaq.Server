using Serilog;
using Serilog.Extensions.Logging;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Ngaq.Core.Infra;



var builder = WebApplication.CreateBuilder(args);

var logger = Log.Logger = new LoggerConfiguration()
  .Enrich.FromLogContext()
  .WriteTo.Console()
  .CreateLogger();

//logger.Information("Starting web host");

//builder.AddLoggerConfigs();

// var appLogger = new SerilogLoggerFactory(logger)
// 	.CreateLogger<Program>();

//builder.Services.AddOptionConfigs(builder.Configuration, appLogger, builder);
//builder.Services.AddServiceConfigs(appLogger, builder);
//ServiceConfigs.AddServiceConfigs(builder.Services, appLogger, builder);

//Cfg_Service.AddServiceConfigs(builder.Services, appLogger, builder);

// builder.Services.AddFastEndpoints()
// 	.SwaggerDocument(o => {
// 		o.ShortSchemaNames = true;
// 	})
// ;

builder.Services.ConfigureHttpJsonOptions(o=>{
	o.SerializerOptions.TypeInfoResolver = AppJsonCtx.Default;
	o.SerializerOptions.PropertyNamingPolicy = null;
});

var app = builder.Build();
//app.UseFastEndpoints();
//await app.UseAppMiddlewareAndSeedDatabase();
app.MapGet("/", async()=>{
// 	var ans = JSON.parse(
// """
// {"Email":"1","PhoneNumber":"","Password":"1","VerificationCode":"","Captcha":""}
// """
// 		,AppJsonCtx.Default.Req_Register
// 	);

	return "Hello World!";
});


app.MapPost("/Auth/Register", async(HttpContext ctx, CancellationToken ct)=>{
	return "1";
	// var _svc_Register = ctx.RequestServices.GetRequiredService<I_Svc_Register>();
	// var req = await ctx.Request.ReadFromJsonAsync<Req_Register>();
	// if(req == null){
	// 	ctx.Response.StatusCode = 400;
	// 	return Results.BadRequest("parameter error");
	// }
	// var ans = await _svc_Register.RegisterAsy(req, ct);
	// if(ans.Ok){
	// 	return Results.Ok(ans);
	// }
	// //ans.ErrToStr();
	// return Results.BadRequest(ans);
});

app.MapPost("/Auth/Login", async(HttpContext ctx, CancellationToken ct)=>{
	return "2";
	// var svc_Login = ctx.RequestServices.GetRequiredService<I_Svc_Login>();
	// var req = await ctx.Request.ReadFromJsonAsync<Req_Login>();
	// if(req == null){
	// 	ctx.Response.StatusCode = 400;
	// 	return Results.BadRequest("parameter error");
	// }
	// var ans = await svc_Login.LoginAsy(req, ct);
	// if(ans.Ok){
	// 	return Results.Ok(ans);
	// }
	// return Results.BadRequest(ans);
});

app.Run();
// Make the implicit Program.cs class public, so integration tests can reference the correct assembly for host building
public partial class Program { }
