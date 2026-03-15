using Tsinswreng.CsSql;

namespace Ngaq.Biz.Db.TswG.Migrations;

/// 服務端對外公開遷移入口（兼容包裝）。
/// 真正通用遷移執行邏輯在 CsSql 的 MigrationRunner。
public class ServerDbMigrator: MigrationRunner{
	public ServerDbMigrator(
		IMigrationMgr MigrationMgr
		,ISqlCmdMkr SqlCmdMkr
		,IMkrTxn MkrTxn
		,IRepo<SchemaHistory, i64> RepoSchemaHistory
		,TxnWrapper TxnWrapper
	):base(MigrationMgr, SqlCmdMkr, MkrTxn, RepoSchemaHistory, TxnWrapper){}
}
