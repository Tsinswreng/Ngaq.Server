namespace Ngaq.Web;
using Ngaq.Biz.Domains.User.Dto;
using Ngaq.Biz.Domains.User.Svc;
using Ngaq.Core.Infra.Url;

public class TokenValidationMiddleware {
	private readonly RequestDelegate _next;
	private readonly SvcToken SvcToken;
	public TokenValidationMiddleware(
		RequestDelegate next
		,SvcToken SvcToken
	){
		_next = next;
		this.SvcToken = SvcToken;
	}
	public async Task InvokeAsync(HttpContext context, CT Ct) {
		// 只针对 /api/* 做验证
		if (context.Request.Path.StartsWithSegments(
				ConstUrl.Api+"", StringComparison.OrdinalIgnoreCase
			)
		){
			if (!context.Request.Headers.TryGetValue("Authorization", out var header) ||
				!header.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) {
				context.Response.StatusCode = StatusCodes.Status401Unauthorized;
				return;
			}

			var token = header.ToString()["Bearer ".Length..].Trim();
			var R = await SvcToken.ValidateAccessTokenAsy(new ReqValidateAccessToken{
				AccessToken = token
			}, Ct);
			if(!R.Ok){
				context.Response.StatusCode = StatusCodes.Status401Unauthorized;
				return;
			}
			context.User = R.Data?.ClaimsPrincipal!;
			// if (!_validator.Validate(token, out var principal)) {
			// 	context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			// 	return;
			// }
			// context.User = principal;
		}

		await _next(context);
	}
}
