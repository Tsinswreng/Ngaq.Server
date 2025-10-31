namespace Ngaq.Web.User;

using Ngaq.Core.Infra.Url;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Ngaq.Biz.Domains.User.Svc;
using Ngaq.Core.Shared.User.Models.Req;
using Ngaq.Core.Infra;
using Tsinswreng.CsCore;

using U = Ngaq.Core.Infra.Url.ConstUrl.UrlOpenUser;
using UrlUser = Ngaq.Core.Infra.Url.ConstUrl.UrlUser;
using Ngaq.Biz.Domains.User;
using Microsoft.Extensions.Caching.Distributed;

public partial class CtrlrOpenUser(
	SvcUser SvcUser
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
			return await Task.FromResult(Results.Ok(new Tempus()));
		});

		R.MapGet("/TestRedis", async(HttpContext Ctx, CT Ct)=>{
			var Cache = Ctx.RequestServices.GetRequiredService<IDistributedCache>();
			await Cache.SetStringAsync("key", new Tempus().ToIso(), Ct);
			var R = await Cache.GetStringAsync("key", Ct);
			return await Task.FromResult(Results.Ok(R));
		});

		R.MapPost(U.AddUser, AddUser);
		R.MapPost(UrlUser.TokenRefresh, RefreshToken);
		return NIL;
	}

	public async Task<IResult> Login(ReqLogin Req, HttpContext Ctx, CT Ct){
		var R = await SvcUser.Login(
			Ctx.ToUserCtx()
			,Req, Ct
		);
		//return Results.Ok(JSON.stringify(R));
		return this.Ok(R);
	}

	public async Task<IResult> AddUser(ReqAddUser ReqAddUser, HttpContext Ctx, CT Ct){
		await SvcUser.AddUser(Ctx.ToUserCtx(), ReqAddUser, Ct);
		return this.Ok();
	}

	public async Task<IResult> RefreshToken(ReqRefreshTheToken Req, HttpContext Ctx, CT Ct){
		var R = await SvcToken.ValidateEtRefreshTheToken(Ctx.ToUserCtx(), Req.RefreshToken, Ct);
		return this.Ok(R);
	}


}

