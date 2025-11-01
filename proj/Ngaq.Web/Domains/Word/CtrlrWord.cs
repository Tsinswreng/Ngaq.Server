namespace Ngaq.Web.Domains.Word;

using Ngaq.Biz.Domains.User;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Tools;
using Ngaq.Core.Word.Svc;

using U = Ngaq.Core.Infra.Url.ConstUrl.UrlWord;

public class CtrlrWord(
	ISvcWord SvcWord
) : ICtrlr{
	public nil InitRouter(
		RouteGroupBuilder R
	){
		R.MapPost(U.Push, ReceiveFull);
		return NIL;
	}

	public async Task<IResult> ReceiveFull(
		HttpContext Ctx, CT Ct
	){
		using var ms = new MemoryStream();
		await Ctx.Request.Body.CopyToAsync(ms, Ct);
		var Body = ms.ToArray();
		var textWithBlob = ToolTextWithBlob.Parse(Body);
		var info = JSON.parse<WordsPackInfo>(textWithBlob.Text);
		if(info is null){
			throw ItemsErr.Common.ArgErr.ToErr();
		}
		var Req = info.ToDtoCompressedWords(textWithBlob.Blob.ToArray());
		await SvcWord.AddCompressedWord(Ctx.ToUserCtx(), Req, Ct);
		return Results.Ok();
	}
}
