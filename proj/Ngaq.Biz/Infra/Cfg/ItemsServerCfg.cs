namespace Ngaq.Biz.Infra.Cfg;

using Ngaq.Core.Infra;
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
		public static ICfgItem _Redis => Mk(Db, [nameof(_Redis)], null);
		public class Redis{
			public static ICfgItem<str> Host => Mk(_Redis, [nameof(Host)], "localhost");
			public static ICfgItem<i32> Port => Mk(_Redis, [nameof(Port)], 6379);
			public static ICfgItem<str> InstanceName => Mk(_Redis, [nameof(InstanceName)], "");
			public static ICfgItem<str> Password => Mk(_Redis, [nameof(Password)], "");
		}
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
	public class Debug{
		public static ICfgItem _R = Mk(null, [nameof(Debug)]);
		public class Auth{
			public static ICfgItem _R = Mk(null, [nameof(Auth)]);
			public static ICfgItem<str> UserId = Mk(_R, [nameof(UserId)], "1");
			public static ICfgItem<str> AllowedAccessToken => Mk(_R, [nameof(AllowedAccessToken)], "TsinswrengAccessToken");
			public static ICfgItem<str> AllowedRefreshToken => Mk(_R, [nameof(AllowedRefreshToken)], "TsinswrengRefreshToken");
		}
	}

	//public static ICfgItem<str> JwtSecret => Mk(Auth, [nameof(JwtSecret)], "");
}
