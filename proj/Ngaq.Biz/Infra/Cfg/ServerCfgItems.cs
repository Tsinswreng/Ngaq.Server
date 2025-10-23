namespace Ngaq.Biz.Infra.Cfg;

using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Cfg;
using Tsinswreng.CsCfg;
using static Tsinswreng.CsCfg.CfgItem<obj?>;
public partial class ItemsServerCfg{
	protected static ItemsServerCfg? _Inst = null;
	public static ItemsServerCfg Inst => _Inst??= new ItemsServerCfg();
	protected str ConnectionString = nameof(ConnectionString);

	public static ICfgItem<i32> Port => Mk(null, [nameof(Port)], 5000);
	public static ICfgItem Db => Mk(null, [nameof(Db)]);
		public static ICfgItem Postgres => Mk(Db, [nameof(Postgres)], null);
			public static ICfgItem<str> PgDbConnStr => Mk(
				Postgres,[nameof(ConnectionString)],""
			);
			public static ICfgItem<str> PgServer = Mk(
				Postgres, ["Server"], "localhost"
			);
			public static ICfgItem<i32> PgPort = Mk(
				Postgres, ["Port"], 5432
			);
			public static ICfgItem<str> PgDatabase = Mk(
				Postgres, ["Database"], "postgres"
			);
			public static ICfgItem<str> PgUserId = Mk(
				Postgres, ["UserId"], "postgres"
			);
			public static ICfgItem<str> PgPassword = Mk(
				Postgres, ["Password"], ""
			);
		//~Postgres
		public static ICfgItem Redis => Mk(Db, [nameof(Redis)], null);
			public static ICfgItem<str> RedisHost => Mk(Redis, ["Host"], "localhost");
			public static ICfgItem<i32> RedisPort => Mk(Redis, ["Port"], 6379);
			public static ICfgItem<str> RedisInstanceName => Mk(Redis, ["InstanceName"], "");
		//~Redis
		public static ICfgItem Sqlite => Mk(Db, [nameof(Sqlite)], null);
			public static ICfgItem<str> SqliteDbPath => Mk(Sqlite, ["Path"], "ErrDb.sqlite");
		//~Sqlite
	//~Db

	public static ICfgItem _Auth => Mk(null, [nameof(Auth)]);
	public class Auth{
		public static ICfgItem<str> JwtSecret => Mk(_Auth, [nameof(JwtSecret)], "");
		public static ICfgItem<i64> AccessTokenExpiryMs => Mk(
			_Auth, [nameof(AccessTokenExpiryMs)]
			,InMillisecond.Minute * 30
		);
		public static ICfgItem<i64> RefreshTokenExpiryMs => Mk(
			_Auth, [nameof(AccessTokenExpiryMs)]
			,InMillisecond.Day * 7
		);
		public static ICfgItem<str> JwtIssuer => Mk(
			_Auth, [nameof(JwtIssuer)], "Ngaq.Server"
		);

		public static ICfgItem<str> JwtAudience => Mk(
			_Auth, [nameof(JwtAudience)], "Ngaq.Client"
		);
	}
	//public static ICfgItem<str> JwtSecret => Mk(Auth, [nameof(JwtSecret)], "");
}
