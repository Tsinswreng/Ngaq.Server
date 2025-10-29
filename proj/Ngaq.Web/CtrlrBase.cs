namespace Ngaq.Web;

public partial interface ICtrlr{
	public nil InitRouter(
		RouteGroupBuilder R
	);
}

public abstract class BaseCtrlr: ICtrlr{
	public abstract nil InitRouter(
		RouteGroupBuilder R
	);

	public nil Register(){
		
		return NIL;
	}
}

// public abstract class BaseCtrlr:ICtrlr{
// 	public abstract nil InitRouter(
// 		RouteGroupBuilder R
// 	);
// }
