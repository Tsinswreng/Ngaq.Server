using Tsinswreng.CsSql;

namespace Ngaq.Biz.Db.TswG.Migrations;

/// Biz 端遷移清單註冊（僅保留業務遷移選擇）。
public static class ServerMigrations{
	extension(IMigrationMgr z){
		/// 註冊 Biz 端全部遷移。
		/// 通用去重策略由 CsSql.ExtnMigrationMgr 提供。
		public IMigrationMgr UseServerMigrations(){
			z.AddMigrationsIfAbsent([
				new M2026_0315_StudyPlan()
			]);
			return z;
		}
	}
}
