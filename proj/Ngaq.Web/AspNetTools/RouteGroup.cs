#if false
namespace Ngaq.Web.AspNetTools;

using Rtn = object;
public partial class RouteGroup(RouteGroupBuilder Rt):IRouteGroup{
	public IRouteGroup MapGroup(string Url){
		var Raw = Rt.MapGroup(Url);
		return new RouteGroup(Raw);
	}
	[Obsolete("恐不兼容AOT")]
	public Rtn MapGet(string Url, Delegate Handler){
//Unable to statically resolve endpoint handler method. Only method groups, lambda expressions or readonly fields/variables are allowed. Compile-time endpoint generation will skip this endpoint and the endpoint will be generated at runtime. For more information, please see https://aka.ms/aspnet/rdg-known-issuesRDG002
//Using member 'Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions.MapGet(IEndpointRouteBuilder, String, Delegate)' which has 'RequiresUnreferencedCodeAttribute' can break functionality when trimming application code. This API may perform reflection on the supplied delegate and its parameters. These types may be trimmed if not directly referenced
//Using member 'Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions.MapGet(IEndpointRouteBuilder, String, Delegate)' which has 'RequiresDynamicCodeAttribute' can break functionality when AOT compiling. This API may perform reflection on the supplied delegate and its parameters. These types may require generated code and aren't compatible with native AOT applications.(IL3050)
		return Rt.MapGet(Url, Handler);
	}

	[Obsolete("恐不兼容AOT")]
	public Rtn MapPost(string Url, Delegate Handler){
		return Rt.MapPost(Url, Handler);
	}

	public Rtn MapGet(string Url, RequestDelegate Handler){
		return Rt.MapGet(Url, Handler);
	}

	public Rtn MapPost(string Url, RequestDelegate Handler){
		return Rt.MapPost(Url, Handler);
	}

#region MapGet
public Rtn MapGet<R>(string Url, Func<R> Handler){
	return Rt.MapGet(Url, Handler);
}
public Rtn MapGet<A0, R>(string Url, Func<A0, R> Handler){
	return Rt.MapGet(Url, Handler);
}
public Rtn MapGet<A0, A1, R>(string Url, Func<A0, A1, R> Handler){
	return Rt.MapGet(Url, Handler);
}
public Rtn MapGet<A0, A1, A2, R>(string Url, Func<A0, A1, A2, R> Handler){
	return Rt.MapGet(Url, Handler);
}
public Rtn MapGet<A0, A1, A2, A3, R>(string Url, Func<A0, A1, A2, A3, R> Handler){
	return Rt.MapGet(Url, Handler);
}
public Rtn MapGet<A0, A1, A2, A3, A4, R>(string Url, Func<A0, A1, A2, A3, A4, R> Handler){
	return Rt.MapGet(Url, Handler);
}
public Rtn MapGet<A0, A1, A2, A3, A4, A5, R>(string Url, Func<A0, A1, A2, A3, A4, A5, R> Handler){
	return Rt.MapGet(Url, Handler);
}
public Rtn MapGet<A0, A1, A2, A3, A4, A5, A6, R>(string Url, Func<A0, A1, A2, A3, A4, A5, A6, R> Handler){
	return Rt.MapGet(Url, Handler);
}
public Rtn MapGet<A0, A1, A2, A3, A4, A5, A6, A7, R>(string Url, Func<A0, A1, A2, A3, A4, A5, A6, A7, R> Handler){
	return Rt.MapGet(Url, Handler);
}
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
