using Tsinswreng.CsSql;

namespace Ngaq.Server.Db.TswG.Migrations;

/// 服務端對外公開遷移入口（兼容包裝）。
/// 真正通用遷移執行邏輯在 CsSql 的 MigrationRunner。
///
/// 保留此類的目的：
/// - 兼容既有服務端調用點命名
/// - 讓 Biz 層仍可暴露一個語義更明確的入口類名
///
/// 若未來不再需要兼容名稱，可直接由調用方改用 `MigrationRunner`。
public class ServerDbMigrator: MigrationRunner{
	/// 僅轉發到基類，避免在 Biz 層重複實現通用遷移執行流程。
	public ServerDbMigrator(
		IMigrationMgr MigrationMgr
		,ISqlCmdMkr SqlCmdMkr
		,IMkrTxn MkrTxn
		,IRepo<SchemaHistory, i64> RepoSchemaHistory
		,TxnWrapper TxnWrapper
	):base(MigrationMgr, SqlCmdMkr, MkrTxn, RepoSchemaHistory, TxnWrapper){}
}
