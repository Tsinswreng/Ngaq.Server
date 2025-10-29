namespace Ngaq.Web;

// WordEndpoints.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;


file static class BearerTokenFilter{
    // 這裡就是你要塞的「我已經寫好的驗證邏輯」
    internal static async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext ctx,
        EndpointFilterDelegate next)
    {
        var httpCtx = ctx.HttpContext;

        // 舉例：手動抓 Header 驗證
        if (!httpCtx.Request.Headers.TryGetValue("Authorization", out var auth)
            || !auth.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Results.Unauthorized();
        }

        // 把 token 傳進你自己寫好的驗證方法
        var token = auth.ToString()["Bearer ".Length..].Trim();
        if (!await MyTokenValidator.Instance.ValidateAsync(token))
        {
            return Results.Unauthorized();
        }

        // 通過就繼續
        return await next(ctx);
    }
}

public static class WordEndpoints
{
    public static IEndpointRouteBuilder MapWordEndpoints(this IEndpointRouteBuilder app)
    {
        // 需要驗證的端點
        app.MapPost(U.Push, ReceiveFull)
           .AddEndpointFilter(BearerTokenFilter.InvokeAsync)
           .WithName("PushWord")
           .WithOpenApi();   // 如要 Swagger 文件

        // 不需要驗證的端點（例如登入）
        app.MapPost("/api/auth/login", Login)
           .AllowAnonymous()
           .WithName("Login")
           .WithOpenApi();

        return app;
    }

    // 需要驗證的 Handler
    private static async Task<IResult> ReceiveFull(
        ReqAddCompressedWords Req,
        ISvcWord svc,
        HttpContext ctx,
        CancellationToken ct)
    {
        await svc.AddCompressedWord(ctx.ToUserCtx(), Req, ct);
        return Results.Ok();
    }

    // 不需要驗證的 Handler
    private static IResult Login(LoginRequest req)
    {
        // … 登入邏輯
        return Results.Ok(new { Token = "xxxx" });
    }
}
