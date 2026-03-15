using Ngaq.Core.Infra;
using Ngaq.Local.Db.TswG;
using Tsinswreng.CsSql;

namespace Ngaq.Biz.Db.TswG.Migrations;

public class M2026_0315_StudyPlan:SqlMigrationInfo{
	public override i64 CreatedMs{get;set;} = Tempus.FromIso("2026-03-15T12:00:00+08:00");

	public M2026_0315_StudyPlan(){
		Init();
	}

	void Init(){
		var Mgr = new ServerTblMgr();
		LocalTblMgrIniter.InitStudyPlan(Mgr);
		SqlsUp = Mgr.SqlsMkSchema();
	}
}
