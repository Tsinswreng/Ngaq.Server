namespace Ngaq.Web;
using Ngaq.Biz.Domains.User.Dto;
using Ngaq.Biz.Domains.User.Svc;
using Ngaq.Core.Infra.Url;

public class TokenValidationMiddleware {
	private readonly RequestDelegate _next;
	private readonly ISvcToken SvcToken;
	public TokenValidationMiddleware(
		RequestDelegate next
		,ISvcToken SvcToken
	){
		_next = next;
		this.SvcToken = SvcToken;
	}
	public async Task InvokeAsync(HttpContext Ctx) {
		// 只针对 /api/* 做验证
		CT Ct = Ctx.RequestAborted;
		if (Ctx.Request.Path.StartsWithSegments(
				"/Api"+"", StringComparison.OrdinalIgnoreCase
			)
		){
			if (!Ctx.Request.Headers.TryGetValue("Authorization", out var header) ||
				!header.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
			){
				Ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
				return;
			}

			var token = header.ToString()["Bearer ".Length..].Trim();
			var R = await SvcToken.ValidateAccessTokenAsy(new ReqValidateAccessToken{
				AccessToken = token
			}, Ct);
			if(!R.Ok){
				Ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
				return;
			}
			Ctx.User = R.Data?.ClaimsPrincipal!;
			// if (!_validator.Validate(token, out var principal)) {
			// 	context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			// 	return;
			// }
			// context.User = principal;
		}

		await _next(Ctx);
	}
}
