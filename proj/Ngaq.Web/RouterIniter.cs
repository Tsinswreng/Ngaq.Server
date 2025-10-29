namespace Ngaq.Web;

using Ngaq.Web.Domains.Word;
using Ngaq.Web.User;


public partial class AppRouterIniter{

	protected static AppRouterIniter? _Inst = null;
	public static AppRouterIniter Inst => _Inst??= new AppRouterIniter();

	public CtrlrRegisterer CtrlrRegisterer = null!;
	public nil Init(IServiceCollection SvcColct){
		this.CtrlrRegisterer = new(SvcColct);
		RegisterCtrlr();
		return NIL;
	}

	public nil RegisterCtrlr(){
		//TODO 用源生成器掃描ICtrlr接口
		CtrlrRegisterer.RegisterCtrlr<CtrlrOpenUser>();
		CtrlrRegisterer.RegisterCtrlr<CtrlrWord>();
		return NIL;
	}

	public nil InitRouters(
		IServiceProvider SvcPrvdr
		,RouteGroupBuilder BaseRoute
	){
		CtrlrRegisterer.InitRouters(SvcPrvdr, BaseRoute);
		return NIL;
	}
}
