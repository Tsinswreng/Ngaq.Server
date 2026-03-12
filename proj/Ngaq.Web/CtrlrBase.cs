using Ngaq.Core.Infra;
using Tsinswreng.CsErr;

namespace Ngaq.Web;

public partial interface ICtrlr{
	public nil InitRouter(
		RouteGroupBuilder R
	);
}

public static class ExtnICtrlr{
	public static IResult Ok(this ICtrlr z, obj? O=null){
		IWebAns<obj> Ans = new WebAns{
			Data = O
		};
		return Results.Ok(Ans);
	}
}
