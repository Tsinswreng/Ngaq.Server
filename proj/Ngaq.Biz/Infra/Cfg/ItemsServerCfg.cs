namespace Ngaq.Server.Infra.Cfg;

using Ngaq.Core.Infra;
using Tsinswreng.CsCfg;
using static Tsinswreng.CsCfg.CfgNode<obj?>;
public partial class ItemsServerCfg{
	protected static ItemsServerCfg? _Inst = null;
	public static ItemsServerCfg Inst => _Inst??= new ItemsServerCfg();
	protected str ConnectionString = nameof(ConnectionString);

	public static ICfgNode<i32> Port => Mk(null, [nameof(Port)], 5000);
	public static ICfgNode Db => Mk(null, [nameof(Db)]);
		public static ICfgNode Postgres => Mk(Db, [nameof(Postgres)], null);
			public static ICfgNode<str> PgDbConnStr => Mk(
				Postgres,[nameof(ConnectionString)],""
			);
			public static ICfgNode<str> PgServer = Mk(
				Postgres, ["Server"], "localhost"
			);
			public static ICfgNode<i32> PgPort = Mk(
				Postgres, ["Port"], 5432
			);
			public static ICfgNode<str> PgDatabase = Mk(
				Postgres, ["Database"], "postgres"
			);
			public static ICfgNode<str> PgUserId = Mk(
				Postgres, ["UserId"], "postgres"
			);
			public static ICfgNode<str> PgPassword = Mk(
				Postgres, ["Password"], ""
			);
		//~Postgres
		public static ICfgNode _Redis => Mk(Db, [nameof(_Redis)], null);
		public class Redis{
			public static ICfgNode<str> Host => Mk(_Redis, [nameof(Host)], "localhost");
			public static ICfgNode<i32> Port => Mk(_Redis, [nameof(Port)], 6379);
			public static ICfgNode<str> InstanceName => Mk(_Redis, [nameof(InstanceName)], "");
			public static ICfgNode<str> Password => Mk(_Redis, [nameof(Password)], "");
		}
		//~Redis
		public static ICfgNode Sqlite => Mk(Db, [nameof(Sqlite)], null);
			public static ICfgNode<str> SqliteDbPath => Mk(Sqlite, ["Path"], "ErrDb.sqlite");
		//~Sqlite
	//~Db

	public static ICfgNode _Auth => Mk(null, [nameof(Auth)]);
	public class Auth{
		public static ICfgNode<str> JwtSecret => Mk(_Auth, [nameof(JwtSecret)], "");
		public static ICfgNode<i64> AccessTokenExpiryMs => Mk(
			_Auth, [nameof(AccessTokenExpiryMs)]
			,InMillisecond.Minute * 30
		);
		public static ICfgNode<i64> RefreshTokenExpiryMs => Mk(
			_Auth, [nameof(AccessTokenExpiryMs)]
			,InMillisecond.Day * 7
		);
		public static ICfgNode<str> JwtIssuer => Mk(
			_Auth, [nameof(JwtIssuer)], "Ngaq.Server"
		);

		public static ICfgNode<str> JwtAudience => Mk(
			_Auth, [nameof(JwtAudience)], "Ngaq.Client"
		);
	}
	public class Debug{
		public static ICfgNode _R = Mk(null, [nameof(Debug)]);
		public class Auth{
			public static ICfgNode _R = Mk(null, [nameof(Auth)]);
			public static ICfgNode<str> UserId = Mk(_R, [nameof(UserId)], "1");
			public static ICfgNode<str> AllowedAccessToken => Mk(_R, [nameof(AllowedAccessToken)], "TsinswrengAccessToken");
			public static ICfgNode<str> AllowedRefreshToken => Mk(_R, [nameof(AllowedRefreshToken)], "TsinswrengRefreshToken");
		}
	}

	//public static ICfgItem<str> JwtSecret => Mk(Auth, [nameof(JwtSecret)], "");
}
