using Ngaq.Backend.Db.TswG;
using Tsinswreng.CsCore;
using Tsinswreng.CsSql;

namespace Ngaq.Server.Db.TswG.Migrations;

public class FullInit: IMigration{
	public ISqlCmdMkr SqlCmdMkr;
	public ITblMgr TblMgr;
	public TxnWrapper TxnWrapper;
	public IRepo<SchemaHistory, i64> RepoSchemaHistory{get;set;}
	[Impl]
	public IList<str> SqlsUp{get;} = new List<str>();
	[Impl]
	public IList<str> SqlsDown{get;} = new List<str>();

	public FullInit(
		ISqlCmdMkr SqlCmdMkr
		,TxnWrapper TxnWrapper
		,ITblMgr TblMgr
		, IRepo<SchemaHistory, i64> RepoSchemaHistory
	){
		this.RepoSchemaHistory = RepoSchemaHistory.UseCsSqlSrefl();
		this.SqlCmdMkr = SqlCmdMkr;
		this.TxnWrapper = TxnWrapper;
		this.TblMgr = TblMgr;
		SqlUp = TblMgr.SqlMkSchema();
	}

	[Impl]
	public i64 CreatedMs{get;set;} = 1760270025478;
	public str SqlUp = "";
	
	/// version after ran this migration
	
	//public str Version{get;set;}

	public async Task<Func<
		CT, Task<nil>
	>> FnMkSchema(IDbFnCtx Ctx, CT Ct){
	var SqlCmd = await SqlCmdMkr.MkCmd(Ctx, SqlUp, Ct);
		return async(Ct)=>{
			try{
				await SqlCmd.WithCtx(Ctx).All1d(Ct);
				return NIL;
			}catch(Exception e){
				throw new Exception(
					"MkSchema failed\nSql:\n"
					+SqlUp+"\n"
					,e
				);
			}
		};
	}

	public async Task<Func<
		CT, Task<nil>
	>> FnInit(IDbFnCtx Ctx, CT Ct){
		var MkSchema = await FnMkSchema(Ctx, Ct);
		var InsertSchemaHistory = await RepoSchemaHistory.FnInsertManyNoPrepare(Ctx, Ct);
		return async(Ct)=>{
			var SchemaHistory = new SchemaHistory{
				CreatedMs = this.CreatedMs
				,Name = "Init"
			};
			await MkSchema(Ct);
			await InsertSchemaHistory([SchemaHistory], Ct);
			return NIL;
		};
	}

	public async Task<nil> Up(CT Ct){
		return await TxnWrapper.Wrap(FnInit, Ct);
	}
	public async Task<nil> Down(CT Ct){
		throw new NotImplementedException();
	}

	public Task<Func<CT, Task<object>>> FnUpAsy(IDbFnCtx Ctx, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<Func<CT, Task<object>>> FnDownAsy(IDbFnCtx Ctx, CT Ct) {
		throw new NotImplementedException();
	}
}
