using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Ngaq.Biz.Svc;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Core;
using Ngaq.Core.Model.Sys.Req;
using Ngaq.Web.AspNetTools;
using Tsinswreng.CsCore;

namespace Ngaq.Web.User;

public class CtrlrUser(
	SvcUser SvcUser
)
	:ICtrlr
{
	public str BaseUrl{get;set;} = "/User";

	[Impl]
	public nil InitRouter(
		IRouteGroup R
	){
		var U = ApiUrl_User.Inst;
		R = R.MapGroup(BaseUrl);
		R.MapPost(U.Login, Login);

		R.MapGet("/Time", async(HttpContext Ctx, CT Ct)=>{
			//throw new Exception("abc");//t
			return await Task.FromResult(Results.Ok(DateTime.Now));
		});

		R.MapPost(U.Register, Register);
		return NIL;
	}

	public async Task<IResult> Login(ReqLogin Req, HttpContext Ctx){
		return await Task.FromResult(Results.Ok());
	}

	public async Task<IResult> Register(ReqAddUser ReqAddUser, HttpContext Ctx, CT Ct){
		await SvcUser.AddUser(ReqAddUser, Ct);
		return Results.Ok();
	}



}
