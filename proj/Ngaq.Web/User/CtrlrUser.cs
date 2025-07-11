using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Ngaq.Biz.Svc;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Core;
using Ngaq.Core.Model.Sys.Req;
using Ngaq.Core.Models.Sys.Req;
using Ngaq.Core.Tools;
using Ngaq.Web.AspNetTools;
using Tsinswreng.CsCore;

namespace Ngaq.Web.User;

public class CtrlrUser(
	SvcUser SvcUser
)
	:ICtrlr
{
	public str BaseUrl{get;set;} = ConstApiUrl.Inst.ApiV1SysUser;

	[Impl]
	public nil InitRouter(
		IRouteGroup R
	){
		var U = ApiUrl_User.Inst;
		R = R.MapGroup(BaseUrl);
		R.MapPost(U.Login, Login);

		R.MapGet("/Time", async(HttpContext Ctx, CT Ct)=>{
			return await Task.FromResult(Results.Ok(DateTime.Now));
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

