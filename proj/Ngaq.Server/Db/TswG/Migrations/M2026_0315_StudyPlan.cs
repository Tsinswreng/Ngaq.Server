using Ngaq.Core.Infra;
using Ngaq.Local.Db.TswG;
using Tsinswreng.CsSql;

namespace Ngaq.Server.Db.TswG.Migrations;

/// Biz 端的 StudyPlan 結構遷移。
///
/// 設計說明：
/// - 這條遷移只描述「新增哪些結構」
/// - 不直接手寫 DDL，而是復用最新版映射生成 SQL
/// - 這樣可避免手寫建表 SQL 與 TblMgrIniter 定義不一致
public class M2026_0315_StudyPlan:SqlMigrationInfo{
	/// 這條遷移的版本號。
	/// 一旦發佈，不應再修改。
	public override i64 CreatedMs{get;set;} = Tempus.FromIso("2026-03-15T12:00:00+08:00");

	public M2026_0315_StudyPlan(){
		Init();
	}

	/// 使用一個臨時 `ServerTblMgr`，只註冊本次遷移涉及的結構，
	/// 再從映射直接生成 `SqlsUp`。
	void Init(){
		var Mgr = new ServerTblMgr();
		LocalTblMgrIniter.InitStudyPlan(Mgr);
		SqlsUp = Mgr.SqlsMkSchema();
	}
}
