namespace Ngaq.Biz.Infra.Cfg;

using Ngaq.Core.Infra.Cfg;
using Tsinswreng.CsCfg;
using static Tsinswreng.CsCfg.ExtnCfgItem;
public partial class ServerCfgItems{
	protected static ServerCfgItems? _Inst = null;
	public static ServerCfgItems Inst => _Inst??= new ServerCfgItems();
	protected str ConnectionString = nameof(ConnectionString);
	public ICfgItem PostgreSql = Mk([nameof(PostgreSql)]);

	public ICfgItem<i32> Port = Mk([nameof(Port)], 5000);

	public ICfgItem<str> PgDbConnStr = Mk(
		[nameof(PostgreSql)
		,nameof(ConnectionString)]
		,""
	);

}
