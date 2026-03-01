namespace Ngaq.Biz.Db.TswG;
using Ngaq.Core.Shared.User.Models.Po.Device;
using Ngaq.Core.Shared.User.Models.Po.RefreshToken;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.User.Models.Po;
using Ngaq.Core.Infra;
using Ngaq.Core.Model.Po;
using Ngaq.Core.Model.Po.Role;
using Ngaq.Core.Model.Sys.Po.Password;
using Ngaq.Core.Model.Sys.Po.RefreshToken;
using Ngaq.Core.Models.Sys.Po.Password;
using Ngaq.Core.Models.Sys.Po.Permission;
using Ngaq.Core.Models.Sys.Po.Role;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Local.Db.TswG;
using Tsinswreng.CsSqlHelper;
using Tsinswreng.CsTools;
using Ngaq.Core.Shared.User.Models.Bo.Device;
using Tsinswreng.CsCore;

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

	public ITblSetter<T> Mk<T>(str DbTblName){
		return LocalTblMgrIniter.Mk<T>(DbTblName);
	}

	public ITblSetter<T> CfgPoBase<T>(ITblSetter<T> Tbl)
		where T:IPoBase, new()
	{
		return LocalTblMgrIniter.CfgPoBase(Tbl);
	}

	protected bool _Inited{get;set;} = false;

	public nil Init(){
		Mgr.AddTbl(new SchemaHistoryTblMkr().MkTbl());
		LocalTblMgrIniter.Mgr = Mgr;
		LocalTblMgrIniter.Init();


		var TblUser = Mk<PoUser>("User");
		Mgr.AddTbl(TblUser.Tbl);
		{
			var o = TblUser;
			CfgPoBase(o);
			LocalTblMgrIniter.CfgBizCreateUpdateTime(o);
			o.Col(nameof(PoUser.Id)).MapType(IdUser.MkTypeMapFn());
			var optUxUniqueName = new OptMkIdx{
				Unique = true
				, Where =
					o.Tbl.SqlIsNonDel()
					+ $" AND {o.Tbl.Fld(nameof(PoUser.UniqueName))} IS NOT NULL"
					+ $" AND {o.Tbl.Fld(nameof(PoUser.UniqueName))} <> ''"
			};
			o.Idx(optUxUniqueName, [nameof(PoUser.UniqueName)]);

			var optUxEmail = new OptMkIdx{Unique = true, Where = o.Tbl.SqlIsNonDel()};
			o.Idx(optUxEmail, [nameof(PoUser.Email)]);
		}

		var TblPassword = Mk<PoPassword>("Password");
		Mgr.AddTbl(TblPassword);
		{
			var o = TblPassword;
			CfgPoBase(o);
			o.Col(nameof(PoPassword.Id)).MapType(IdPassword.MkTypeMapFn());
			o.Col(nameof(PoPassword.Algo)).MapEnumToInt32<PoPassword.EAlgo>();
			o.Col(nameof(PoPassword.UserId)).MapType(IdUser.MkTypeMapFn());
		}

		var TblRole = Mk<PoRole>("Role");
		Mgr.AddTbl(TblRole);
		{
			var o = TblRole;
			CfgPoBase(o);
			o.Col(nameof(PoRole.Id)).MapType(IdRole.MkTypeMapFn());
			o.Col(nameof(PoRole.Status)).MapEnumToInt32<PoRole.ERoleStatus>();
			o.Idx(new OptMkIdx{Unique = true}, [nameof(PoRole.Code)]);
		}

		var TblPermission = Mk<PoPermission>("Permission");
		Mgr.AddTbl(TblPermission);
		{
			var o = TblPermission;
			CfgPoBase(o);
			o.Col(nameof(PoPermission.Id)).MapType(IdPermission.MkTypeMapFn());
			o.Idx(new OptMkIdx{Unique = true}, [nameof(PoPermission.Code)]);
		}

		var TblRefreshToken = Mk<PoRefreshToken>("RefreshToken");
		Mgr.AddTbl(TblRefreshToken);
		{
			var o = TblRefreshToken;
			CfgPoBase(o);
			LocalTblMgrIniter.CfgBizCreateUpdateTime(o);
			o.Col(nameof(PoRefreshToken.Id)).MapType(IdRefreshToken.MkTypeMapFn());
			o.Col(nameof(PoRefreshToken.UserId)).MapType(IdUser.MkTypeMapFn());
			o.Col(nameof(PoRefreshToken.ClientId)).MapType(IdClient.MkTypeMapFn());
			o.Col(nameof(PoRefreshToken.ExpireAt)).MapType(MapTempus());
			o.Col(nameof(PoRefreshToken.RevokeAt)).MapType(MapTempus());
			o.Col(nameof(PoRefreshToken.LastUsedAt)).MapType(MapTempus());
			o.Col(nameof(PoRefreshToken.TokenValueType)).MapEnumToStr<PoRefreshToken.ETokenValueType>();
			o.Col(nameof(PoRefreshToken.ClientType)).MapEnumToStr<EClientType>();
			o.IdxExpr(null, x=>x.UserId);

		}

		_Inited = true;
		return NIL;
	}
}
