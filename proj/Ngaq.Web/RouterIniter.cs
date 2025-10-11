namespace Ngaq.Web;
using Ngaq.Web.User;


public partial class AppRouterIniter{
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
		,RouteGroupBuilder BaseRoute
	){
		CtrlrRegisterer.InitRouters(SvcPrvdr, BaseRoute);
		return NIL;
	}
}
