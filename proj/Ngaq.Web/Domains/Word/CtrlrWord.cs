namespace Ngaq.Web.Domains.Word;

using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Tools;
using Ngaq.Core.Word.Svc;
using Ngaq.Web.Infra;
using U = Ngaq.Core.Infra.Url.ConstUrl.UrlWord;

public class CtrlrWord(
	ISvcWord SvcWord
) : ICtrlr{
	public nil InitRouter(
		RouteGroupBuilder R
	){
		R.MapPost(U.Push, ReceiveFull);
		R.MapPost(U.Pull, SendFull);

		return NIL;
	}

	public async Task<IResult> ReceiveFull(
		HttpContext Ctx, CT Ct
	){
		using var ms = new MemoryStream();
		await Ctx.Request.Body.CopyToAsync(ms, Ct);
		var Body = ms.ToArray();
		var textWithBlob = ToolTextWithBlob.Parse(Body);
		await SvcWord.SyncFromTextWithBlob(Ctx.ToUserCtx(), textWithBlob, Ct);
		//return Results.Ok();
		return this.Ok();
	}

	public async Task<IResult> SendFull(
		ReqPackWords Req
		,HttpContext Ctx, CT Ct
	){
		var textWithBlob = await SvcWord.PackAllWordsToTextWithBlobNoStream(
			Ctx.ToUserCtx(), Req, Ct
		);
		var bytes = textWithBlob.ToByteArr();
		return Results.Bytes(bytes);
	}
}
