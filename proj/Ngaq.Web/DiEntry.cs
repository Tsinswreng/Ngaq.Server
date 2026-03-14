namespace Ngaq.Web;

using Ngaq.Biz;
using Ngaq.Biz.Infra.Cfg;
using Ngaq.Core;
using Tsinswreng.CsCfg;
using CfgItems = Ngaq.Biz.Infra.Cfg.ItemsServerCfg;



/// 蔿入口之依賴注入

public static class DiEntry{
	public static IServiceCollection SetupEntry(
		this IServiceCollection z
		,ICfgAccessor Cfg
	){
		z.SetupCore()
		.SetupBiz()
		.SetupWeb();
		return z;
	}
}
