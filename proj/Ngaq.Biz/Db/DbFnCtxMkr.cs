using Ngaq.Local.Db;
using Tsinswreng.CsSqlHelper;

namespace Ngaq.Biz.Db;

// public interface DbFnCtxMkr<TDbFnCtx>
// 	:IDbFnCtxMkr<TDbFnCtx>
// 	where TDbFnCtx: IDbFnCtx, new()
// {

// }

public class DbFnCtxMkr<TDbFnCtx>
	:BaseDbFnCtxMkr<TDbFnCtx>
	where TDbFnCtx: IBaseDbFnCtx, new()
{
	public DbFnCtxMkr(I_GetTxnAsy GetTxn):base(GetTxn){}
}
