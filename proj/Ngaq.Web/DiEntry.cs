namespace Ngaq.Web;

using Ngaq.Biz;
using Ngaq.Biz.Infra.Cfg;
using Tsinswreng.CsCfg;
using CfgItems = Ngaq.Biz.Infra.Cfg.ItemsServerCfg;



public static class DiEntry{
	public static IServiceCollection Setup(
		this IServiceCollection z
		,ICfgAccessor Cfg
	){
		z.SetupBiz()
		.SetupWeb()
		.AddStackExchangeRedisCache(opt=>{
			var RedisConnStr = CfgItems.Redis.Host.GetFrom(Cfg)+":"+CfgItems.Redis.Port.GetFrom(Cfg);
			opt.Configuration = RedisConnStr;
			opt.InstanceName = CfgItems.Redis.InstanceName.GetFrom(Cfg);
		});
		return z;
	}
}
