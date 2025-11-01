namespace Ngaq.Web;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ngaq.Biz.Domains.User.Dto;
using Ngaq.Biz.Domains.User.Svc;
using Ngaq.Biz.Infra.Cfg;
using Ngaq.Core.Infra.Url;
using Tsinswreng.CsCfg;

public class TokenValidationMiddleware {
	private readonly RequestDelegate _next;
	ISvcToken SvcToken;
	ICfgAccessor Cfg;

	public TokenValidationMiddleware(
		RequestDelegate next
		,ISvcToken SvcToken
		,ICfgAccessor Cfg
	){
		_next = next;
		this.SvcToken = SvcToken;
		this.Cfg = Cfg;
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
			#if DEBUG //TEMP
			var AllowedAToken = Cfg.Get(ItemsServerCfg.Debug.Auth.AllowedAccessToken);
			if(token == AllowedAToken){
				var UserIdStr = Cfg.Get(ItemsServerCfg.Debug.Auth.UserId);
				Claim[] claims = [
					new Claim(JwtRegisteredClaimNames.Sub, UserIdStr+""),          // OIDC 標準
					new Claim(ClaimTypes.NameIdentifier, UserIdStr+""),            // .NET 傳統
					new Claim(ClaimTypes.Name, $"DebugUser{UserIdStr}"),
				];
				var identity = new ClaimsIdentity(claims, "DebugScheme");
				var principal = new ClaimsPrincipal(identity);
				Ctx.User = principal;
				//這裏怎麼寫?
				await _next(Ctx);
				return;
			}
			#endif
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
