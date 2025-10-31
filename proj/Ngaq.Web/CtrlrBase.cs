using Ngaq.Core.Infra;

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

[Obsolete]
public abstract class BaseCtrlr: ICtrlr{
	// public abstract nil InitRouter(
	// 	RouteGroupBuilder R
	// );
	public nil InitRouter(
		RouteGroupBuilder R
	){return NIL;}

	public nil Register(){

		return NIL;
	}

}

// public abstract class BaseCtrlr:ICtrlr{
// 	public abstract nil InitRouter(
// 		RouteGroupBuilder R
// 	);
// }
