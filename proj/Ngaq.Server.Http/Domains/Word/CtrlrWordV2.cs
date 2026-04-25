namespace Ngaq.Server.Http.Domains.Word;

using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Core.Tools.Json;
using Ngaq.Server.Http.Infra;
using Tsinswreng.CsTools;
using U = Ngaq.Core.Infra.Url.KeysUrl.WordV2;

/// 單詞同步 V2 控制器。
/// 路徑根爲 /Api/V2/Word。
public class CtrlrWordV2(
	ISvcWordV2 SvcWordV2,
	IJsonSerializer JsonS
) : ICtrlr{

	/// 初始化路由映射。
	/// <param name="R">路由組。</param>
	/// <returns>NIL。</returns>
	public nil InitRouter(RouteGroupBuilder R){
		R.MapPost(U.Push, ReceiveFull);
		R.MapPost(U.Pull, SendFull);
		R.MapPost(U.BatSyncJnWordByBizId, BatSyncJnWordByBizId);
		R.MapPost(U.BatAddJnWord, BatAddJnWord);
		R.MapPost(U.GetAllWordsWithDel, GetAllWordsWithDel);
		R.MapPost(U.BatUpdHeadLang, BatUpdHeadLang);
		R.MapPost(U.SoftDelJnWordInId, SoftDelJnWordInId);
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
		return await HandleNoReqRawStream(
			Ct,
			ct=>SvcWordV2.PackAllWordsWithDel(Ctx.ToDbUserCtx(), ct)
		);
	}

	/// 接收 gzip 行流的 JnWord，按 BizId 同步，並返回 gzip 行流的同步結果 DTO。
	/// <param name="Ctx">HTTP 上下文。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>application/octet-stream 的 gzip 壓縮行流。</returns>
	public async Task<IResult> BatSyncJnWordByBizId(HttpContext Ctx, CT Ct){
		return await HandleReqRespAsGzipLines<JnWord, DtoJnWordSyncResult>(
			Ctx,
			Ct,
			(req, ct)=>SvcWordV2.BatSyncJnWordByBizId(Ctx.ToDbUserCtx(), req, ct)
		);
	}

	/// 批量新增 JnWord（gzip 行流入，無返回體）。
	/// <param name="Ctx">HTTP 上下文。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>成功返回空 OK。</returns>
	public async Task<IResult> BatAddJnWord(HttpContext Ctx, CT Ct){
		return await HandleReqNoRespAsOk<JnWord>(
			Ctx,
			Ct,
			(req, ct)=>SvcWordV2.BatAddJnWord(Ctx.ToDbUserCtx(), req, ct)
		);
	}

	/// 取全部單詞（含軟刪），返回 gzip 行流。
	/// <param name="Ctx">HTTP 上下文。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>application/octet-stream 的 gzip 壓縮行流。</returns>
	public async Task<IResult> GetAllWordsWithDel(HttpContext Ctx, CT Ct){
		return await HandleNoReqRespAsGzipLines<JnWord>(
			Ct,
			ct=>SvcWordV2.GetAllWordsWithDel(Ctx.ToDbUserCtx(), ct)
		);
	}

	/// 批量更新 Head/Lang（gzip 行流入，gzip 行流出）。
	/// <param name="Ctx">HTTP 上下文。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>application/octet-stream 的 gzip 壓縮行流。</returns>
	public async Task<IResult> BatUpdHeadLang(HttpContext Ctx, CT Ct){
		return await HandleReqRespAsGzipLines<PoWord, RespUpdBizId>(
			Ctx,
			Ct,
			(req, ct)=>SvcWordV2.BatUpdHeadLang(Ctx.ToDbUserCtx(), req, ct)
		);
	}

	/// 按 Id 批量軟刪單詞（gzip 行流入，無返回體）。
	/// <param name="Ctx">HTTP 上下文。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>成功返回空 OK。</returns>
	public async Task<IResult> SoftDelJnWordInId(HttpContext Ctx, CT Ct){
		return await HandleReqNoRespAsOk<IdWord>(
			Ctx,
			Ct,
			(req, ct)=>SvcWordV2.SoftDelJnWordInId(Ctx.ToDbUserCtx(), req, ct)
		);
	}

	/// 從請求體讀 gzip json 行並反序列化為異步元素流。
	/// <typeparam name="TReq">請求元素類型。</typeparam>
	/// <param name="Ctx">HTTP 上下文。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>反序列化後的元素流。</returns>
	protected IAsyncEnumerable<TReq> ReadReqAsGzipLines<TReq>(HttpContext Ctx, CT Ct){
		return GZipLinesUtf8
			.ToLines(Ctx.Request.Body, Ct)
			.Select(line=>JsonS.Parse<TReq>(line)!);
	}

	/// 把返回元素流序列化成 gzip json 行並作為 octet-stream 響應。
	/// <typeparam name="TResp">返回元素類型。</typeparam>
	/// <param name="Resp">返回元素流。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>HTTP 響應結果。</returns>
	protected async Task<IResult> WriteRespAsGzipLines<TResp>(
		IAsyncEnumerable<TResp> Resp,
		CT Ct
	){
		var lines = Resp.Select(item=>JsonS.Stringify(item));
		var payload = await GZipLinesUtf8.ToStream(lines, Ct);
		return Results.Stream(payload, "application/octet-stream");
	}

	/// 通用模板：gzip 行流請求 + 無返回體，成功返回 OK。
	/// <typeparam name="TReq">請求元素類型。</typeparam>
	/// <param name="Ctx">HTTP 上下文。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <param name="Fn">業務函數。</param>
	/// <returns>HTTP 響應結果。</returns>
	protected async Task<IResult> HandleReqNoRespAsOk<TReq>(
		HttpContext Ctx,
		CT Ct,
		Func<IAsyncEnumerable<TReq>, CT, Task<nil>> Fn
	){
		var req = ReadReqAsGzipLines<TReq>(Ctx, Ct);
		await Fn(req, Ct);
		return this.Ok();
	}

	/// 通用模板：gzip 行流請求 + gzip 行流響應。
	/// <typeparam name="TReq">請求元素類型。</typeparam>
	/// <typeparam name="TResp">返回元素類型。</typeparam>
	/// <param name="Ctx">HTTP 上下文。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <param name="Fn">業務函數。</param>
	/// <returns>HTTP 響應結果。</returns>
	protected async Task<IResult> HandleReqRespAsGzipLines<TReq, TResp>(
		HttpContext Ctx,
		CT Ct,
		Func<IAsyncEnumerable<TReq>, CT, IAsyncEnumerable<TResp>> Fn
	){
		var req = ReadReqAsGzipLines<TReq>(Ctx, Ct);
		var resp = Fn(req, Ct);
		return await WriteRespAsGzipLines(resp, Ct);
	}

	/// 通用模板：無請求體 + gzip 行流響應。
	/// <typeparam name="TResp">返回元素類型。</typeparam>
	/// <param name="Ct">取消令牌。</param>
	/// <param name="Fn">業務函數。</param>
	/// <returns>HTTP 響應結果。</returns>
	protected async Task<IResult> HandleNoReqRespAsGzipLines<TResp>(
		CT Ct,
		Func<CT, IAsyncEnumerable<TResp>> Fn
	){
		var resp = Fn(Ct);
		return await WriteRespAsGzipLines(resp, Ct);
	}

	/// 通用模板：無請求體 + 原始字節流響應。
	/// <param name="Ct">取消令牌。</param>
	/// <param name="Fn">業務函數。</param>
	/// <returns>HTTP 響應結果。</returns>
	protected async Task<IResult> HandleNoReqRawStream(
		CT Ct,
		Func<CT, Task<Stream>> Fn
	){
		var stream = await Fn(Ct);
		return Results.Stream(stream, "application/octet-stream");
	}
}
