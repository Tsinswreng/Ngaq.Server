namespace Ngaq.Biz.Db.TswG;

using Ngaq.Core.Shared.User.Models.Bo.Device;
using Ngaq.Core.Shared.User.Models.Bo.Jwt;
using Ngaq.Core.Shared.User.Models.Po.Device;
using Ngaq.Core.Shared.User.Models.Po.RefreshToken;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Model.Po.Role;
using Ngaq.Core.Model.Sys.Po.Password;
using Ngaq.Core.Model.Sys.Po.RefreshToken;
using Ngaq.Core.Models.Sys.Po.Password;
using Ngaq.Core.Models.Sys.Po.Permission;
using Ngaq.Core.Models.Sys.Po.Role;
using Ngaq.Core.Service.Parser;
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

	public static IUpperTypeMapFnT<i64,Tempus> MapTempus(){
		return UpperTypeMapFnT<i64, Tempus>.Mk(
			raw=>new Tempus(raw)
			,tempus=>tempus.Value
		);
	}

	public static IUpperTypeMapFnT<i64?,Tempus?> MapTempusN(){
		return UpperTypeMapFnT<i64?, Tempus?>.Mk(
			val=>val==null?null:new Tempus(val.Value)
			,tempus=>tempus?.Value
		);
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
WHERE {o.SqlIsNonDel()}
"""
,$"""
CREATE UNIQUE INDEX {o.Qt($"Ux_{o.DbTblName}_EMail")}
ON{o.Qt(o.DbTblName)} ({o.Fld(nameof(PoUser.Email))})
WHERE {o.SqlIsNonDel()}
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
			o.SetCol(nameof(PoRole.Status)).MapEnumTypeInt32<PoRole.ERoleStatus>();
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

		var TblRefreshToken = Mk<PoRefreshToken>("Session");
		Mgr.AddTbl(TblRefreshToken);
		{
			var o = TblRefreshToken;
			CfgPoBase(o);
			o.SetCol(nameof(PoRefreshToken.Id)).MapType(IdRefreshToken.MkTypeMapFn());
			o.SetCol(nameof(PoRefreshToken.UserId)).MapType(IdUser.MkTypeMapFn());
			o.SetCol(nameof(PoRefreshToken.Jti)).MapType(Jti.MkTypeMapFn());
			o.SetCol(nameof(PoRefreshToken.ClientId)).MapType(IdClient.MkTypeMapFn());
			o.SetCol(nameof(PoRefreshToken.ExpireAt)).MapType(MapTempus());
			o.SetCol(nameof(PoRefreshToken.RevokeAt)).MapType(MapTempusN());
			o.SetCol(nameof(PoRefreshToken.LastUsedAt)).MapType(MapTempusN());
			o.SetCol(nameof(PoRefreshToken.TokenValueType)).MapEnumTypeInt32<PoRefreshToken.ETokenValueType>();
			o.SetCol(nameof(PoRefreshToken.ClientType)).MapEnumTypeInt32<EClientType>();

			o.OuterAdditionalSqls.AddRange([
/// JTI 唯一约束（仅存活数据）
$"""
CREATE UNIQUE INDEX {o.Qt($"Ux_{o.DbTblName}_{nameof(PoRefreshToken.Jti)}")}
ON {o.Qt(o.DbTblName)} ({o.Fld(nameof(PoRefreshToken.Jti))})
WHERE {o.SqlIsNonDel()}
""",
//UserId索引
$"""
CREATE INDEX {o.Qt($"Idx_{o.DbTblName}_{nameof(PoRefreshToken.UserId)}")}
ON {o.Qt(o.DbTblName)} ({o.Fld(nameof(PoRefreshToken.UserId))})
"""
			]);

		}

		_Inited = true;
		return NIL;
	}
}
