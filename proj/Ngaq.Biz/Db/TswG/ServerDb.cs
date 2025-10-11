
namespace Ngaq.Biz.Db.TswG;

using System.Data;
using System.Data.Common;
using Ngaq.Biz.Infra.Cfg;
using Npgsql;
using Tsinswreng.CsCfg;
using Tsinswreng.CsSqlHelper.Postgres;
using CFG = Ngaq.Biz.Infra.Cfg.ServerCfgItems;


public partial class ServerDb{
protected static ServerDb? _Inst = null;
public static ServerDb Inst => _Inst??= new ServerDb();

	public ICfgAccessor CfgAccessor{ get; set; } = ServerCfg.Inst;

	//public IDbConnection DbConnection{get;set;}
	public NpgsqlDataSource DataSource{get;set;}
	public PostgresConnPool DbConnPool{get;set;}

	public ServerDb(){
		var Ca = CfgAccessor;
		var DataBase = CFG.PgDatabase.GetFrom(Ca);
		var Port = CFG.PgPort.GetFrom(Ca);
		var Server = CFG.PgServer.GetFrom(Ca);
		var UserId = CFG.PgUserId.GetFrom(Ca);
		var Password = CFG.PgPassword.GetFrom(Ca);
		var ConnStr = $"Server={Server};Port={Port};Database={DataBase};User Id={UserId};Password={Password}";
		//DbConnection = new NpgsqlConnection(ConnStr);
		var DataSrcBuilder = new NpgsqlDataSourceBuilder(ConnStr);
		DataSource = DataSrcBuilder.Build();
		DbConnPool = new PostgresConnPool(DataSource);
		//dsBuilder.UseLoggerFactory(loggerFactory); // 接入 Microsoft.Extensions.Logging
		// 2-B AOT / 源生成器（可选）
		//dsBuilder.EnableDynamicJson();         // System.Text.Json 源生成
		// dsBuilder.EnableNativeAot();        // 如果项目开 AOT
	}

}
