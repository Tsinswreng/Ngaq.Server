namespace Ngaq.Web;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Errors;
using Tsinswreng.CsErr;

public class GlobalErrHandler : IExceptionHandler {
	public async ValueTask<bool> TryHandleAsync(HttpContext Ctx, Exception Err, CT Ct) {
		if(Err is IAppErr E){
			if(
				E.Tags.Contains(AppErrTags.BizErr)
				&& E.Tags.Contains(AppErrTags.Public)
			){
				Ctx.Response.StatusCode = 400; //TODO 潙2開頭旹纔改成400
				Ctx.Body(WebAns.Mk(null, [E])); //TODO 處理內部Errors
			}
			return true;
		}
		Ctx.Response.StatusCode = 500;
		return true;
	}
}
