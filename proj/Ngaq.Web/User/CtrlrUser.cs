namespace Ngaq.Web.User;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Ngaq.Biz.Svc;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Core;
using Ngaq.Core.Models.Sys.Req;
using Ngaq.Core.Tools;
using Ngaq.Web.AspNetTools;
using Tsinswreng.CsCore;


using U = Ngaq.Core.Infra.ConstUrl.UrlUser;
public partial class CtrlrUser(
	SvcUser SvcUser
)
	:ICtrlr
{
	public str BaseUrl{get;set;} =  ConstUrl.User;  //ConstApiUrlOld.Inst.ApiV1SysUser;

	[Impl]
	public nil InitRouter(
		IRouteGroup R
	){
		R = R.MapGroup(BaseUrl);
		R.MapPost(U.Login, Login);

		R.MapGet("/Time", async(HttpContext Ctx, CT Ct)=>{
			return await Task.FromResult(Results.Ok(new Tempus()));
		});

		R.MapPost(U.AddUser, AddUser);
		return NIL;
	}

	public async Task<IResult> Login(ReqLogin Req, HttpContext Ctx, CT Ct){
		var R = await SvcUser.Login(Req, Ct);
		//return Results.Ok(JSON.stringify(R));
		return Results.Ok(R);
	}

	public async Task<IResult> AddUser(ReqAddUser ReqAddUser, HttpContext Ctx, CT Ct){
		await SvcUser.AddUser(ReqAddUser, Ct);
		return Results.Ok();
	}


}

