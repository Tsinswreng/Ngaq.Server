using Ngaq.Web.AspNetTools;
using Ngaq.Web.User;

namespace Ngaq.Web;

public  partial class AppRouterIniter{
	public CtrlrRegisterer CtrlrRegisterer;
	public AppRouterIniter(IServiceCollection SvcColct){
		this.CtrlrRegisterer = new(SvcColct);
		RegisterCtrlr();
	}

	public nil RegisterCtrlr(){
		CtrlrRegisterer.RegisterCtrlr<CtrlrUser>();
		return NIL;
	}

	public nil Init(
		IServiceProvider SvcPrvdr
		,IRouteGroup BaseRoute
	){
		CtrlrRegisterer.InitRouters(SvcPrvdr, BaseRoute);
		return NIL;
	}
}
