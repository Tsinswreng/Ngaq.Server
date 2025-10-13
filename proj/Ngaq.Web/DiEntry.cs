using Ngaq.Biz;
using Ngaq.Biz.Infra.Cfg;
using Tsinswreng.CsCfg;
using CfgItems = Ngaq.Biz.Infra.Cfg.ServerCfgItems;

namespace Ngaq.Web;

public static class DiEntry{
	public static IServiceCollection Setup(
		this IServiceCollection z
		,ICfgAccessor Cfg
	){
		z.SetupBiz()
		.SetupWeb()
		.AddStackExchangeRedisCache(opt=>{
			var RedisConnStr = CfgItems.RedisHost.GetFrom(Cfg)+":"+CfgItems.RedisPort.GetFrom(Cfg);
			opt.Configuration = RedisConnStr;
			opt.InstanceName = CfgItems.RedisInstanceName.GetFrom(Cfg);
		});
		return z;
	}
}
