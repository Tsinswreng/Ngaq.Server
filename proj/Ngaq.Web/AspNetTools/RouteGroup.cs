namespace Ngaq.Web.AspNetTools;

using Rtn = object;
public class RouteGroup(RouteGroupBuilder R):IRouteGroup{
	public IRouteGroup MapGroup(string Url){
		var Raw = R.MapGroup(Url);
		return new RouteGroup(Raw);
	}
	public Rtn MapGet(string Url, Delegate Handler){
		return R.MapGet(Url, Handler);
	}
	public Rtn MapPost(string Url, Delegate Handler){
		return R.MapPost(Url, Handler);
	}
}
