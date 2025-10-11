namespace Ngaq.Biz.Db.TswG;

using Ngaq.Core.Model.Po.Role;
using Ngaq.Core.Model.Sys.Po.Password;
using Ngaq.Core.Model.Sys.Po.User;
using Ngaq.Core.Models.Sys.Po.Password;
using Ngaq.Core.Models.Sys.Po.Permission;
using Ngaq.Core.Models.Sys.Po.Role;
using Ngaq.Core.Models.Sys.Po.User;
using Ngaq.Local.Db.TswG;
using Tsinswreng.CsSqlHelper;
using Tsinswreng.CsTools;


public partial class ServerTblMgrIniter{

	public ITblMgr Mgr{get;set;}
	LocalTblMgrIniter LocalTblMgrIniter;
	public ServerTblMgrIniter(
		ITblMgr Mgr
		,LocalTblMgrIniter LocalTblMgrIniter
	){
		this.Mgr = Mgr;
		this.LocalTblMgrIniter = LocalTblMgrIniter;
	}

	public ITable Mk<T>(str DbTblName){
		return LocalTblMgrIniter.Mk<T>(DbTblName);
	}

	public ITable CfgPoBase(ITable Tbl){
		return LocalTblMgrIniter.CfgPoBase(Tbl);
	}

	protected bool _Inited{get;set;} = false;

	public nil Init(){
		Mgr.AddTbl(new SchemaHistoryTblMkr().MkTbl());
		LocalTblMgrIniter.Mgr = Mgr;
		LocalTblMgrIniter.Init();


		var TblUser = Mk<PoUser>("User");
		Mgr.AddTbl(TblUser);
		{
			var o = TblUser;
			CfgPoBase(o);
			o.SetCol(nameof(PoUser.Id)).MapType(IdUser.MkTypeMapFn());
			o.OuterAdditionalSqls.AddRange([
$"""
CREATE UNIQUE INDEX {o.Qt($"Ux_{o.DbTblName}_UniqueName")}
ON {o.Qt(o.DbTblName)} ({o.Fld(nameof(PoUser.UniqueName))})
WHERE {o.Fld(nameof(PoUser.DelId))} IS NULL
"""
,$"""
CREATE UNIQUE INDEX {o.Qt($"Ux_{o.DbTblName}_EMail")}
ON{o.Qt(o.DbTblName)} ({o.Fld(nameof(PoUser.Email))})
WHERE {o.Fld(nameof(PoUser.DelId))} IS NULL
"""
			]);
		}

		var TblPassword = Mk<PoPassword>("Password");
		Mgr.AddTbl(TblPassword);
		{
			var o = TblPassword;
			CfgPoBase(o);
			o.SetCol(nameof(PoPassword.Id)).MapType(IdPassword.MkTypeMapFn());
			o.SetCol(nameof(PoPassword.Algo)).MapEnumTypeInt32<PoPassword.EAlgo>();
			o.SetCol(nameof(PoPassword.UserId)).MapType(IdUser.MkTypeMapFn());
		}

		var TblRole = Mk<PoRole>("Role");
		Mgr.AddTbl(TblRole);
		{
			var o = TblRole;
			CfgPoBase(o);
			o.SetCol(nameof(PoRole.Id)).MapType(IdRole.MkTypeMapFn());
			o.SetCol(nameof(PoRole.Id)).MapEnumTypeInt32<PoRole.ERoleStatus>();
			o.OuterAdditionalSqls.AddRange([
$"""
CREATE UNIQUE INDEX {o.Qt($"Ux_{o.DbTblName}_Code")}
ON {o.Qt(o.DbTblName)} ({o.Fld(nameof(PoRole.Code))})
"""
			]);
		}

		var TblPermission = Mk<PoPermission>("Permission");
		Mgr.AddTbl(TblPermission);
		{
			var o = TblPermission;
			CfgPoBase(o);
			o.SetCol(nameof(PoPermission.Id)).MapType(IdPermission.MkTypeMapFn());
			o.OuterAdditionalSqls.AddRange([
$"""
CREATE UNIQUE INDEX {o.Qt($"Ux_{o.DbTblName}_Code")}
ON {o.Qt(o.DbTblName)} ({o.Fld(nameof(PoPermission.Code))})
"""
			]);
		}
		_Inited = true;
		return NIL;
	}
}
