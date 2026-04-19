namespace Ngaq.Server.Http;

using Ngaq.Server;
using Ngaq.Server.Infra.Cfg;
using Ngaq.Core;
using Tsinswreng.CsCfg;
using CfgItems = Ngaq.Server.Infra.Cfg.KeysServerCfg;
using Ngaq.Backend.Di;



/// 蔿入口之依賴注入

public static class DiHttpServer{
	public static IServiceCollection SetupHttpServer(
		this IServiceCollection z
		,ICfgAccessor Cfg
	){
		z.SetupCore()
		.SetupServer()
		.SetupWeb();
		return z;
	}
}
