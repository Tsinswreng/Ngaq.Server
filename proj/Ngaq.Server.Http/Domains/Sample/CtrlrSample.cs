namespace Ngaq.Server.Http.Domains.Sample;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Ngaq.Core.Shared.User.Models.Req;
using Tsinswreng.CsCore;
using Ngaq.Server.Http.Infra;
using Ngaq.Core.Shared.User.Models.Resp;
using Ngaq.Server.Http;
using Ngaq.Core.Shared.User.UserCtx;

using U = Core.Infra.Url.KeysUrl.OpenUser;

[Doc(@$"模擬的 後端服務接口")]
public interface ISvcSample{
	public Task<RespLogin> Login(IUserCtx User, ReqLogin Req, CT Ct);
}

public partial class CtrlrSample( // controller 要有 Ctrlr 前綴
	ISvcSample SvcSample // 主構造函數注入
)
	:ICtrlr // controller 要實現此接口
{

	
	[Doc(@$"實現 ICtrlr 的 方法。在此方法中註冊 路由 對 控制器方法 的映射。
	{nameof(U)} 是 路由鍵的類型別名
	")]
	[Impl]
	public nil InitRouter(
		RouteGroupBuilder R
	){
		R.MapPost(U.Login, Login);
		return NIL;
	}


	[Doc(@$"具體控制器方法。
	- 函數返值必須聲明成 `Task<IResult>`
	- 最後兩個參數必須聲明成 `HttpContext Ctx, CT Ct`
	- 第一個參數是請求體參數
	使用 `{nameof(Rtn)}` 標記 實際的強類型的返回值結構。
	如果不返回實際數據就寫`{nameof(Rtn)}(typeof(nil))` 然後接 `return this.Ok();`
	")]
	[Rtn(typeof(RespLogin))]
	public async Task<IResult> Login(ReqLogin Req, HttpContext Ctx, CT Ct){
		var R = await SvcSample.Login(
			Ctx.ToUserCtx()
			,Req, Ct
		);
		//成功返回時的寫法:
		return this.Ok(R);
	}
}

