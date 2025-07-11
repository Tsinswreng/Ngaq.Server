namespace Ngaq.Biz.Infra.Cfg;

using Ngaq.Core.Infra.Cfg;
using Tsinswreng.CsCfg;
using static Tsinswreng.CsCfg.ExtnCfgItem;
public partial class ServerCfgItems{
	protected static ServerCfgItems? _Inst = null;
	public static ServerCfgItems Inst => _Inst??= new ServerCfgItems();
	protected str ConnectionString = nameof(ConnectionString);

	public ICfgItem<i32> Port => Mk([nameof(Port)], 5000);
	public ICfgItem Db => Mk([nameof(Db)]);
	public ICfgItem PostgreSql => Mk([nameof(PostgreSql)], null, Db);
	public ICfgItem<str> PgDbConnStr => Mk(
		[nameof(ConnectionString)],"",PostgreSql
	);

	public ICfgItem Sqlite => Mk([nameof(Sqlite)], null, Db);
	public ICfgItem<str> SqliteDbPath => Mk(["Path"], "ErrDb.sqlite", Sqlite);

	public ICfgItem User => Mk([nameof(User)]);
	public ICfgItem<str> JwtSecret => Mk([nameof(JwtSecret)], "", User);
}
