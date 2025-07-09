using Ngaq.Local.Db;
using Tsinswreng.CsSqlHelper;

namespace Ngaq.Biz.Db;

public class EfTxnRunner:ITxnRunner{
	public async Task<TRet> RunTxn<TRet>(
		ITxn Txn
		,Func<
			CT, Task<TRet>
		> FnAsy
		, CT Ct
	){
		return await FnAsy(Ct);
	}
}


public class DbFnCtxMkr(
	I_GetTxnAsy GetTxn
)
{
	public async Task<DbFnCtx> MkTxnDbFnCtxAsy(CT Ct){
		var R = new DbFnCtx();
		R.Txn = await GetTxn.GetTxnAsy(Ct);
		return R;
	}
}
