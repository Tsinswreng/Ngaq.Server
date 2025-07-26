namespace Ngaq.Biz.Infra.Cfg;

using Ngaq.Core.Infra.Cfg;
using Tsinswreng.CsCfg;
using static Tsinswreng.CsCfg.ExtnCfgItem;
public partial class ServerCfgItems{
	protected static ServerCfgItems? _Inst = null;
	public static ServerCfgItems Inst => _Inst??= new ServerCfgItems();
	protected str ConnectionString = nameof(ConnectionString);

	public static ICfgItem<i32> Port => Mk(null, [nameof(Port)], 5000);
	public static ICfgItem Db => Mk(null, [nameof(Db)]);
		public static ICfgItem PostgreSql => Mk(Db, [nameof(PostgreSql)], null);
			public static ICfgItem<str> PgDbConnStr => Mk(
				PostgreSql,[nameof(ConnectionString)],""
			);
		//~PostgreSql
		public static ICfgItem Redis => Mk(Db, [nameof(Redis)], null);
			public static ICfgItem<str> RedisHost => Mk(Redis, ["Host"], "localhost");
			public static ICfgItem<i32> RedisPort => Mk(Redis, ["Port"], 6379);
			public static ICfgItem<str> RedisInstanceName => Mk(Redis, ["InstanceName"], "");
		//~Redis
		public static ICfgItem Sqlite => Mk(Db, [nameof(Sqlite)], null);
			public static ICfgItem<str> SqliteDbPath => Mk(Sqlite, ["Path"], "ErrDb.sqlite");
		//~Sqlite
	//~Db

	public static ICfgItem User => Mk(null, [nameof(User)]);
		public static ICfgItem<str> JwtSecret => Mk(User, [nameof(JwtSecret)], "");
}
