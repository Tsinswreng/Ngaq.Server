using Ngaq.Local.Db;
using Tsinswreng.CsSqlHelper;

namespace Ngaq.Biz.Db;

// public interface DbFnCtxMkr<TDbFnCtx>
// 	:IDbFnCtxMkr<TDbFnCtx>
// 	where TDbFnCtx: IDbFnCtx, new()
// {

// }

public class DbFnCtxMkr<TDbFnCtx>
	:IDbFnCtxMkr<TDbFnCtx>
	where TDbFnCtx: IDbFnCtx, new()
{
	public I_GetTxnAsy TxnGetter{get;set;}
	public DbFnCtxMkr(I_GetTxnAsy GetTxn){
		this.TxnGetter = GetTxn;
	}
}
