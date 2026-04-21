namespace Ngaq.Server.Http.Domains.Word;

using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Server.Http.Infra;
using U = Ngaq.Core.Infra.Url.KeysUrl.WordV2;

/// 單詞同步 V2 控制器。
/// 路徑根爲 /Api/V2/Word。
public class CtrlrWordV2(
	ISvcWordV2 SvcWordV2
) : ICtrlr{

	/// 初始化路由映射。
	/// <param name="R">路由組。</param>
	/// <returns>NIL。</returns>
	public nil InitRouter(RouteGroupBuilder R){
		R.MapPost(U.Push, ReceiveFull);
		R.MapPost(U.Pull, SendFull);
		return NIL;
	}

	/// 接收客戶端上傳的完整詞庫流並按 BizId 規則同步入庫。
	/// <param name="Ctx">HTTP 上下文。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>成功返回空 OK。</returns>
	public async Task<IResult> ReceiveFull(HttpContext Ctx, CT Ct){
		await foreach(var _ in SvcWordV2.BatSyncJnWordByBizIdFromStream(
			Ctx.ToDbUserCtx(),
			Ctx.Request.Body,
			Ct
		)){
			// 消費枚舉以觸發同步執行。
		}
		return this.Ok();
	}

	/// 打包當前用戶全部單詞（含軟刪）並以字節流返回。
	/// <param name="Ctx">HTTP 上下文。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>字節流響應。</returns>
	public async Task<IResult> SendFull(HttpContext Ctx, CT Ct){
		var packed = await SvcWordV2.PackAllWordsWithDel(Ctx.ToDbUserCtx(), Ct);
		return Results.Stream(packed, "application/octet-stream");
	}
}
