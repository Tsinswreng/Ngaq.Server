namespace Ngaq.Web;

using Ngaq.Biz;
using Ngaq.Biz.Infra.Cfg;
using Tsinswreng.CsCfg;
using CfgItems = Ngaq.Biz.Infra.Cfg.ItemsServerCfg;


/// <summary>
/// 蔿入口之依賴注入
/// </summary>
public static class DiEntry{
	public static IServiceCollection Setup(
		this IServiceCollection z
		,ICfgAccessor Cfg
	){
		z.SetupBiz()
		.SetupWeb();
		return z;
	}
}
