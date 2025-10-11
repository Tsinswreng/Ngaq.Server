#if false
namespace Ngaq.Web.AspNetTools;

using Rtn = object;
public partial interface IRouteGroup{
	public Rtn MapGet(string Url, RequestDelegate Handler);
	public Rtn MapPost(string Url, RequestDelegate Handler);


#region MapGet
public Rtn MapGet<R>(string Url, Func<R> Handler);
public Rtn MapGet<A0, R>(string Url, Func<A0, R> Handler);
public Rtn MapGet<A0, A1, R>(string Url, Func<A0, A1, R> Handler);
public Rtn MapGet<A0, A1, A2, R>(string Url, Func<A0, A1, A2, R> Handler);
public Rtn MapGet<A0, A1, A2, A3, R>(string Url, Func<A0, A1, A2, A3, R> Handler);
public Rtn MapGet<A0, A1, A2, A3, A4, R>(string Url, Func<A0, A1, A2, A3, A4, R> Handler);
public Rtn MapGet<A0, A1, A2, A3, A4, A5, R>(string Url, Func<A0, A1, A2, A3, A4, A5, R> Handler);
public Rtn MapGet<A0, A1, A2, A3, A4, A5, A6, R>(string Url, Func<A0, A1, A2, A3, A4, A5, A6, R> Handler);
public Rtn MapGet<A0, A1, A2, A3, A4, A5, A6, A7, R>(string Url, Func<A0, A1, A2, A3, A4, A5, A6, A7, R> Handler);
#endregion MapGet

#region MapPost
public Rtn MapPost<R>(string Url, Func<R> Handler);
public Rtn MapPost<A0, R>(string Url, Func<A0, R> Handler);
public Rtn MapPost<A0, A1, R>(string Url, Func<A0, A1, R> Handler);
public Rtn MapPost<A0, A1, A2, R>(string Url, Func<A0, A1, A2, R> Handler);
public Rtn MapPost<A0, A1, A2, A3, R>(string Url, Func<A0, A1, A2, A3, R> Handler);
public Rtn MapPost<A0, A1, A2, A3, A4, R>(string Url, Func<A0, A1, A2, A3, A4, R> Handler);
public Rtn MapPost<A0, A1, A2, A3, A4, A5, R>(string Url, Func<A0, A1, A2, A3, A4, A5, R> Handler);
public Rtn MapPost<A0, A1, A2, A3, A4, A5, A6, R>(string Url, Func<A0, A1, A2, A3, A4, A5, A6, R> Handler);
public Rtn MapPost<A0, A1, A2, A3, A4, A5, A6, A7, R>(string Url, Func<A0, A1, A2, A3, A4, A5, A6, A7, R> Handler);
#endregion MapPost


}

#endif
