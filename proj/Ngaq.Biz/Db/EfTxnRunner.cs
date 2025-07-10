using Ngaq.Local.Db;
using Tsinswreng.CsSqlHelper;

namespace Ngaq.Biz.Db;

public class EfTxnRunner:ITxnRunner{
	public async Task<TRet> RunTxn<TRet>(
		ITxn? Txn
		,Func<
			CT, Task<TRet>
		> FnAsy
		, CT Ct
	){
		if(Txn == null){
			TRet R = await FnAsy(Ct);
			return R;
		}
		try{
			await Txn.Begin(Ct);
			TRet ans = await FnAsy(Ct);
			await Txn.Commit(Ct);
			return ans;
		}
		catch (Exception) {
			await Txn.Rollback(Ct);
			throw;
		}
	}
}


public class DbFnCtxMkr<TDbFnCtx>(
	I_GetTxnAsy GetTxn
)where TDbFnCtx: IDbFnCtx, new()
{
	public async Task<TDbFnCtx> MkTxnDbFnCtxAsy(CT Ct){
		var R = new TDbFnCtx();
		R.Txn = await GetTxn.GetTxnAsy(Ct);
		return R;
	}
}


//TODO 抽至公共庫
public class TxnWrapper<TDbFnCtx>(
	DbFnCtxMkr<TDbFnCtx> DbFnCtxMkr
	,ITxnRunner TxnRunner
)
where TDbFnCtx: IDbFnCtx, new()
{
	//無參(除末ʹCt外)
	public async Task<nil> Wrap<TRet>(
		Func<TDbFnCtx, CT, Task<Func<
			CT
			,Task<TRet>
		>>> FnXxx
		,CT Ct
	){
		var Ctx = await DbFnCtxMkr.MkTxnDbFnCtxAsy(Ct);
		var Xxx = await FnXxx(Ctx, Ct);
		await TxnRunner.RunTxn(Ctx.Txn, async(Ct)=>{
			return await Xxx(Ct);
		}, Ct);
		return NIL;
	}
	//1
	public async Task<nil> Wrap<TArg0, TRet>(
		Func<TDbFnCtx, CT, Task<Func<
			TArg0
			,CT
			,Task<TRet>
		>>> FnXxx
		,TArg0 Arg0
		,CT Ct
	){
		var Ctx = await DbFnCtxMkr.MkTxnDbFnCtxAsy(Ct);
		var Xxx = await FnXxx(Ctx, Ct);
		await TxnRunner.RunTxn(Ctx.Txn, async(Ct)=>{
			return await Xxx(Arg0, Ct);
		}, Ct);
		return NIL;
	}
	//2
	public async Task<nil> Wrap<TArg0, TArg1, TRet>(
		Func<TDbFnCtx, CT, Task<Func<
			TArg0
			,TArg1
			,CT
			,Task<TRet>
		>>> FnXxx
		,TArg0 Arg0
		,TArg1 Arg1
		,CT Ct
	){
		var Ctx = await DbFnCtxMkr.MkTxnDbFnCtxAsy(Ct);
		var Xxx = await FnXxx(Ctx, Ct);
		await TxnRunner.RunTxn(Ctx.Txn, async(Ct)=>{
			return await Xxx(Arg0, Arg1, Ct);
		}, Ct);
		return NIL;
	}



}
