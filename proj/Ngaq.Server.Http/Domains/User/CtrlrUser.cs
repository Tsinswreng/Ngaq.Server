namespace Ngaq.Server.Http.Domains.User;

using Ngaq.Core.Infra.Url;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Ngaq.Server.Domains.User.Svc;
using Ngaq.Core.Shared.User.Svc;
using Ngaq.Core.Shared.User.Models.Req;
using Ngaq.Core.Infra;
using Tsinswreng.CsCore;

using U = Core.Infra.Url.KeysUrl.OpenUser;
using ApiUser = Core.Infra.Url.KeysUrl.ApiUser;
using Microsoft.Extensions.Caching.Distributed;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Tools;
using Ngaq.Server.Http.Infra;
using Tsinswreng.CsErr;
using Ngaq.Core.Models.Sys.Req;
using Ngaq.Core.Shared.User.Models.Resp;
using Tsinswreng.CsTempus;

public partial class CtrlrOpenUser(
	ISvcUser SvcUser
	,ISvcToken SvcToken
)
	:ICtrlr
{
	//public str BaseUrl{get;set;} =  ConstUrl.User;  //ConstApiUrlOld.Inst.ApiV1SysUser;

	[Impl]
	public nil InitRouter(
		RouteGroupBuilder R
	){
		//R = R.MapGroup(BaseUrl);
		R.MapPost(U.Login, Login);

		R.MapGet("/Time", async(HttpContext Ctx, CT Ct)=>{
			return await Task.FromResult(Results.Ok(new UnixMs()));
		});
		R.MapGet("/Open/Time", async(HttpContext Ctx, CT Ct)=>{
			return this.Ok(new UnixMs());
		});

		R.MapGet("/TestRedis", async(HttpContext Ctx, CT Ct)=>{
			var Cache = Ctx.RequestServices.GetRequiredService<IDistributedCache>();
			await Cache.SetStringAsync("key", new UnixMs().ToIso(), Ct);
			var R = await Cache.GetStringAsync("key", Ct);
			return await Task.FromResult(Results.Ok(R));
		});

		R.MapPost(U.AddUser, AddUser);
		R.MapPost(U.TokenRefresh, RefreshToken);
		R.MapPost(ApiUser.Logout, Logout);
		return NIL;
	}


	[Rtn(typeof(RespLogin))]
	public async Task<IResult> Login(ReqLogin Req, HttpContext Ctx, CT Ct){
		var R = await SvcUser.Login(
			Ctx.ToUserCtx()
			,Req, Ct
		);
		//return Results.Ok(JSON.stringify(R));
		return this.Ok(R);
	}

	[Rtn(typeof(nil))]
	public async Task<IResult> AddUser(ReqAddUser ReqAddUser, HttpContext Ctx, CT Ct){
		await SvcUser.AddUser(Ctx.ToUserCtx(), ReqAddUser, Ct);
		return this.Ok();
	}

	[Rtn(typeof(RespRefreshBothToken))]
	public async Task<IResult> RefreshToken(ReqRefreshTheToken Req, HttpContext Ctx, CT Ct){
		var Ans = await SvcToken.ValidateEtRefreshTheToken(Ctx.ToUserCtx(), Req.RefreshToken, Ct);
		var R = Ans.DataOrThrow();
		return this.Ok(R);
	}

	[Rtn(typeof(nil))]
	public async Task<IResult> Logout(ReqLogout Req, HttpContext Ctx, CT Ct){
		var R = await SvcUser.Logout(Ctx.ToUserCtx(), Req, Ct);
		return this.Ok(R);
	}

}

