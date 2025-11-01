namespace Ngaq.Biz.Domains.User;

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Shared.User.Models.Bo.Device;
using Ngaq.Core.Shared.User.Models.Po.Device;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Core.Tools;

public class ServerUserCtx : IServerUserCtx{
	public IDictionary<str, obj?>? Kv{get;set;}
	public str? IpAddr{get;set;}
	public IdClient? ClientId{get;set;}
	public str? UserAgent{get;set;}
	public EClientType ClientType{get;set;} = EClientType.Unknown;
	public IdUser _UserId = IdUser.Zero;
	public IdUser UserId{get{
		if(_UserId.IsNullOrDefault()){
			throw ItemsErr.User.AuthenticationFailed.ToErr();
		}
		return _UserId;
	}set{
		_UserId = value;
	}}
}

public static class ExtnUserCtx{
	/// <summary>
	/// 驗證用戶id後汶取。宜皆由此㕥取用戶id洏免直ᵈ調IUserCtx.UserId
	/// </summary>
	/// <param name="z"></param>
	/// <returns></returns>
	public static IdUser GetValidUserId(this IUserCtx z){
		var UserId = z.UserId;
		if(UserId.IsNullOrDefault()){
			throw ItemsErr.User.AuthenticationFailed.ToErr();
		}
		return UserId;
	}
	public static IServerUserCtx AsServerUserCtx(
		this IUserCtx z
	){
		if(z is IServerUserCtx R){
			return R;
		}
		throw new InvalidCastException();
	}

	public static IServerUserCtx ToUserCtx(this HttpContext z){
		return new ServerUserCtx().FromHttpCtx(z);
	}

	static IdUser GetUserIdFromClaims(ClaimsPrincipal Principal){
		// 1. 优先从 .NET 映射后的声明（NameIdentifier）获取（当前环境下 sub 会被映射到这里）
		var userIdValue = Principal.FindFirstValue(ClaimTypes.NameIdentifier);
		// 2. 若未获取到，再尝试从原始 JWT 标准声明（sub）获取（兼容可能的配置变更）
		if (string.IsNullOrWhiteSpace(userIdValue)){
			userIdValue = Principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
		}

		// 3. 若仍未获取到有效值，返回默认的 Zero（如公开接口无令牌场景）
		if (string.IsNullOrWhiteSpace(userIdValue)){
			return IdUser.Zero;
		}

		// 4. 安全解析用户 ID（处理可能的格式错误，避免崩溃）
		return IdUser.FromLow64Base(userIdValue);
		// try{

		// }
		// catch (Exception ex){
		// 	// 解析失败时（如值格式不符合 Low64Base 规则），返回 Zero 并可记录日志
		// 	// （根据实际需求决定是否添加日志，此处仅示例）
		// 	// Logger.LogWarning(ex, $"解析用户 ID 失败，原始值：{userIdValue}");
		// 	return IdUser.Zero;
		// }
	}

	public static TSelf FromHttpCtx<TSelf>(
		this TSelf z
		,HttpContext HttpCtx
	)where TSelf:IServerUserCtx
	{
		z.IpAddr = GetClientIp(HttpCtx);

		// 2. User-Agent
		z.UserAgent = HttpCtx.Request.Headers.UserAgent.FirstOrDefault();

		// 3. 客户端标识（自定义 Header 或 Query）
		//    约定：Header 优先，Query 兜底，都没有就 null
		if (HttpCtx.Request.Headers.TryGetValue("X-Client-Id", out var clientIdHdr))
			z.ClientId = IdClient.FromLow64Base(clientIdHdr!);
		else if (HttpCtx.Request.Query.TryGetValue("client_id", out var clientIdQuery))
			z.ClientId = IdClient.FromLow64Base(clientIdQuery!);
		else
			z.ClientId = null;

		// 4. 客户端类型（简单嗅探）
		z.ClientType = SniffClientType(z.UserAgent);
		z.UserId = GetUserIdFromClaims(HttpCtx.User);
		return z;
	}

	#region helpers
	private static string GetClientIp(HttpContext Ctx){
		// 先拿 X-Forwarded-For 第一个 IP（如果经过代理）
		var headers = Ctx.Request.Headers;
		if (headers.TryGetValue("X-Forwarded-For", out var forwarded))
		{
			var first = forwarded.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries)
								.FirstOrDefault()
								?.Trim();
			if (!string.IsNullOrWhiteSpace(first) && IPAddress.TryParse(first, out _)){
				return first;
			}
		}

		// 再拿 X-Real-IP
		if (headers.TryGetValue("X-Real-IP", out var real) &&
			IPAddress.TryParse(real!, out _)
		){
			return real!;
		}
		// 最后拿连接层远程地址
		return Ctx.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
	}

	private static EClientType SniffClientType(string? ua){
		if (string.IsNullOrWhiteSpace(ua)){
			return EClientType.Unknown;
		}

		ua = ua.ToLowerInvariant();
		if (ua.Contains("mobile"))
			return EClientType.Mobile;
		if (ua.Contains("bot") || ua.Contains("spider"))
			return EClientType.Bot;
		if (ua.Contains("postman") || ua.Contains("insomnia"))
			return EClientType.ApiTool;

		return EClientType.Web; // 默认当 PC 浏览器
	}
	#endregion
}
