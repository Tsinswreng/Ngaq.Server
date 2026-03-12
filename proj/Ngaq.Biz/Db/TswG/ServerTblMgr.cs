namespace Ngaq.Biz.Db.TswG;

using Tsinswreng.CsSql.Postgres;

public class ServerTblMgr:PostgresTblMgr{
protected static ServerTblMgr? _Inst = null;
public static ServerTblMgr Inst => _Inst??= new ServerTblMgr();


}
