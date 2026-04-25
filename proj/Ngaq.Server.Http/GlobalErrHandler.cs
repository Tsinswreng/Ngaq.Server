namespace Ngaq.Server.Http;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Errors;
using Tsinswreng.CsErr;

public class GlobalErrHandler : IExceptionHandler {
	ILogger<GlobalErrHandler> Logger{get;}

	public GlobalErrHandler(ILogger<GlobalErrHandler> Logger){
		this.Logger = Logger;
	}

	public async ValueTask<bool> TryHandleAsync(HttpContext Ctx, Exception Err, CT Ct) {
		Logger.LogError(Err, "Unhandled exception. Path={Path}; Method={Method}", Ctx.Request.Path, Ctx.Request.Method);
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
