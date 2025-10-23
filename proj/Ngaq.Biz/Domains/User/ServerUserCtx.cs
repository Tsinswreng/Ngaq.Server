namespace Ngaq.Biz.Domains.User;

using System.Net;
using Microsoft.AspNetCore.Http;
using Ngaq.Core.Shared.User.Models.Bo.Device;
using Ngaq.Core.Shared.User.Models.Po.Device;
using Ngaq.Core.Shared.User.UserCtx;

public class ServerUserCtx : UserCtx, IServerUserCtx{
	public str? IpAddr{get;set;}
	public IdClient? ClientId{get;set;}
	public str? UserAgent{get;set;}
	public EClientType ClientType{get;set;} = EClientType.Unknown;
}

public static class ExtnUseCtx{
	public static IServerUserCtx AsServerUserCtx(
		this IUserCtx z
	){
		if(z is IServerUserCtx R){
			return R;
		}
		throw new InvalidCastException();
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
