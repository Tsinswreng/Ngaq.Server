using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Core;
using Ngaq.Core.Model.Sys.Req;

namespace Ngaq.Web.User;

public class CtrlrUser(
	RouteGroupBuilder R
)
	:BaseCtrlr
{

	public nil Init(){

		var U = ApiUrl_User.Inst;
		R.MapPost(U.Login, Login);
/*
Using member 'Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions.MapPost(IEndpointRouteBuilder, String, Delegate)' which has 'RequiresUnreferencedCodeAttribute' can break functionality when trimming application code. This API may perform reflection on the supplied delegate and its parameters. These types may be trimmed if not directly referenced.(IL2026)
Using member 'Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions.MapPost(IEndpointRouteBuilder, String, Delegate)' which has 'RequiresDynamicCodeAttribute' can break functionality when AOT compiling. This API may perform reflection on the supplied delegate and its parameters. These types may require generated code and aren't compatible with native AOT applications.(IL3050)
 */
		return Nil;
	}

	public async Task<IResult> Login(ReqLogin Req, HttpContext Ctx){
		return await Task.FromResult(Results.Ok());
	}

}
