namespace Ngaq.Web.Midware;

using Ngaq.Web.Infra;
using StackExchange.Redis;
using Tsinswreng.CsCfg;

public class EdgeDebounceMidware {
	RequestDelegate _next;
	IConnectionMultiplexer _redis;
	ILogger<EdgeDebounceMidware> _log;
	TimeSpan _ttl;
	ICfgAccessor Cfg;


	public EdgeDebounceMidware(
		RequestDelegate next,
		IConnectionMultiplexer redis,
		ILogger<EdgeDebounceMidware> log,
		ICfgAccessor Cfg
	){
		this.Cfg = Cfg;
		_next = next;
		_redis = redis;
		_log = log;
		_ttl = TimeSpan.FromMilliseconds(800);//ms, 暫寫死
	}

	public async Task InvokeAsync(HttpContext Ctx) {
		// 1. 只處理可能改變狀態的 Method

		#if false
		if (!HttpMethods.IsPost(Ctx.Request.Method) &&
			!HttpMethods.IsPut(Ctx.Request.Method) &&
			!HttpMethods.IsPatch(Ctx.Request.Method)) {
			await _next(Ctx);
			return;
		}
		#endif
		// 2. 取維度
		var user = Ctx.ToUserCtx();
		var ip = Ctx.Connection.RemoteIpAddress?.ToString();
		if (Ctx.Request.Headers.TryGetValue("X-Forwarded-For", out var xff)){
			ip = xff.ToString().Split(',')[0].Trim();
		}


		var isOpen = Ctx.Request.Path.StartsWithSegments("/Open");
		var dimension = "";
		if(isOpen){
			dimension = $"ip:{ip}";
		}else{
			dimension = $"uid:{user.UserId}";
		}
		// 3. 拼 Key
		var key = $"edge_debounce:{dimension}:{Ctx.Request.Path}";

		// 4. 原子搶鎖
		var db = _redis.GetDatabase();
		if (!await db.StringSetAsync(key, "1", _ttl, When.NotExists)) {//「只有當 key 不存在時」才把值設成 "1"，並給定過期時間 _ttl
			//true → 成功寫入（表示這是 ttl 內第一次請求，放行）。
			//false → key 已存在（表示 ttl 內已經有請求進來，拒絕）。
			Ctx.Response.StatusCode = StatusCodes.Status429TooManyRequests;
			Ctx.Response.Headers["X-Edge-Debounce"] = "hit";   // 標記，方便排查
			await Ctx.Response.WriteAsync("Too fast");
			return;
		}

		// 5. 旁路標記：告訴下游「這請求已經過一次防抖」
		Ctx.Items["__EdgeDebounced"] = true;

		await _next(Ctx);
	}
}
