namespace Ngaq.Web;
using System.Diagnostics.CodeAnalysis;


public partial class CtrlrRegisterer{
	public IServiceCollection SvcColct;
	public CtrlrRegisterer(IServiceCollection SvcColct){
		this.SvcColct = SvcColct;
	}
	public IServiceProvider SvcPrvdr = null!;
	public RouteGroupBuilder BaseRoute = null!;

	IList<Func<nil>> InitFns = new List<Func<nil>>();

	public nil RegisterCtrlr<
//'TService' generic argument does not satisfy 'DynamicallyAccessedMemberTypes.PublicConstructors' in 'Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<TService>(IServiceCollection)'. The generic parameter 'T' of 'Ngaq.Web.CtrlrRegisterer.RegisterCtrlr<T>()' does not have matching annotations. The source value must declare at least the same requirements as those declared on the target location it is assigned to.
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		T
	>()
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
