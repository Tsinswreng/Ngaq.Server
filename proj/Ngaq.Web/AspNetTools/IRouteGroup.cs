namespace Ngaq.Web.AspNetTools;

using Rtn = object;
public  partial interface IRouteGroup{
	public IRouteGroup MapGroup(string Url);
	public Rtn MapGet(string Url, Delegate Handler);
	public Rtn MapPost(string Url, Delegate Handler);
}
