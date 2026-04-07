namespace Ngaq.Server.Http;

using Ngaq.Server;
using Ngaq.Server.Infra.Cfg;
using Ngaq.Core;
using Tsinswreng.CsCfg;
using CfgItems = Ngaq.Server.Infra.Cfg.ItemsServerCfg;



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
