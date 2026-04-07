using Tsinswreng.CsSql;

namespace Ngaq.Server.Db.TswG.Migrations;

/// Biz 端遷移清單註冊（僅保留業務遷移選擇）。
public static class ServerMigrations{
	extension(IMigrationMgr z){
		/// 註冊 Biz 端全部遷移。
		/// 通用去重策略由 CsSql.ExtnMigrationMgr 提供。
		///
		/// 約定：
		/// - 服務端若新增結構變更，只需要在此追加遷移類
		/// - 真正執行由 `MigrationRunner` 完成
		/// - 因爲 Biz 建立在 Local 之上，所以這裏只放 Biz 自己新增的遷移
		public IMigrationMgr UseServerMigrations(){
			z.AddMigrationsIfAbsent([
				// 服務端新增 StudyPlan 相關結構。
				new M2026_0315_StudyPlan()
			]);
			return z;
		}
	}
}
