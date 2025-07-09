namespace Ngaq.Web;

public class CtrlrRegisterer{
	public IServiceCollection SvcColct;
	public CtrlrRegisterer(IServiceCollection SvcColct){
		this.SvcColct = SvcColct;
	}
	public IServiceProvider SvcPrvdr = null!;
	public RouteGroupBuilder BaseRoute = null!;

	IList<Func<nil>> InitFns = new List<Func<nil>>();

	public nil RegisterCtrlr<T>()
		where T:class, ICtrlr
	{
		SvcColct.AddScoped<T>();
		InitFns.Add(()=>{
			SvcPrvdr.GetRequiredService<T>().InitRouter(BaseRoute);
			return NIL;
		});
		return NIL;
	}

	public nil InitRouters(
		IServiceProvider SvcPrvdr
		,RouteGroupBuilder BaseRoute
	){
		this.SvcPrvdr = SvcPrvdr;
		this.BaseRoute = BaseRoute;
		foreach(var fn in InitFns){
			fn();
		}
		return NIL;
	}

}
