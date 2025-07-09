using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Ngaq.Biz.Svc;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Core;
using Ngaq.Core.Model.Sys.Req;
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
		RouteGroupBuilder R
	){
		var U = ApiUrl_User.Inst;
		R.MapPost(U.Login, Login);

		R.MapGet("/Time", async(HttpContext Ctx, CT Ct)=>{
			return await Task.FromResult(Results.Ok(DateTime.Now));
		});

		return NIL;
	}

	public async Task<IResult> Login(ReqLogin Req, HttpContext Ctx){
		return await Task.FromResult(Results.Ok());
	}

}
