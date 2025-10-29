namespace Ngaq.Web.Domains.Word;

using Ngaq.Biz.Domains.User;
using Ngaq.Core.Shared.Word.Models.Dto;
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
		ReqAddCompressedWords Req
		,HttpContext Ctx, CT Ct
	){
		await SvcWord.AddCompressedWord(Ctx.ToUserCtx(), Req, Ct);
		return Results.Ok();
	}
}
