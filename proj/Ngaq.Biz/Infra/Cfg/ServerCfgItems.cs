namespace Ngaq.Biz.Infra.Cfg;

using Ngaq.Core.Infra.Cfg;
using Tsinswreng.CsCfg;
using static Tsinswreng.CsCfg.ExtnCfgItem;
public partial class ServerCfgItems{
	protected static ServerCfgItems? _Inst = null;
	public static ServerCfgItems Inst => _Inst??= new ServerCfgItems();
	protected str ConnectionString = nameof(ConnectionString);

	public ICfgItem<i32> Port => Mk(null, [nameof(Port)], 5000);
	public ICfgItem Db => Mk(null, [nameof(Db)]);
		public ICfgItem PostgreSql => Mk(Db, [nameof(PostgreSql)], null);
			public ICfgItem<str> PgDbConnStr => Mk(
				PostgreSql,[nameof(ConnectionString)],""
			);
		//~PostgreSql
		public ICfgItem Redis => Mk(Db, [nameof(Redis)], null);
			public ICfgItem<str> RedisHost => Mk(Redis, ["Host"], "localhost");
			public ICfgItem<i32> RedisPort => Mk(Redis, ["Port"], 6379);
			public ICfgItem<str> RedisInstanceName => Mk(Redis, ["InstanceName"], "");
		//~Redis
		public ICfgItem Sqlite => Mk(Db, [nameof(Sqlite)], null);
			public ICfgItem<str> SqliteDbPath => Mk(Sqlite, ["Path"], "ErrDb.sqlite");
		//~Sqlite
	//~Db

	public ICfgItem User => Mk(null, [nameof(User)]);
		public ICfgItem<str> JwtSecret => Mk(User, [nameof(JwtSecret)], "");
}
