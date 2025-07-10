using Ngaq.Web.AspNetTools;

namespace Ngaq.Web;

public static class DiWeb{
	public static IServiceCollection SetupWeb(
		this IServiceCollection z
	){
		//z.AddSingleton<IRouteGroup, RouteGroup>();

		return z;
	}
}

