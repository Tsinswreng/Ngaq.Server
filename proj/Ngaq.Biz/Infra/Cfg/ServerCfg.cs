using Ngaq.Core.Infra.Cfg;
using Tsinswreng.CsCfg;

namespace Ngaq.Biz.Infra.Cfg;
public  partial class ServerCfg : JsonFileCfgAccessor, ICfgAccessor{
	protected static ServerCfg? _Inst = null;
	public static ServerCfg Inst => _Inst??= new ServerCfg();

}
