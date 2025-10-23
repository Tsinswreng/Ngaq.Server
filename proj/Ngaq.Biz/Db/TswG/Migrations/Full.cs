using Ngaq.Local.Db.TswG;
using Tsinswreng.CsCore;
using Tsinswreng.CsSqlHelper;

namespace Ngaq.Biz.Db.TswG.Migrations;

public class FullInit: IMigration{
	public ISqlCmdMkr SqlCmdMkr;
	public ITblMgr TblMgr;
	public TxnWrapper<DbFnCtx> TxnWrapper;
	public IAppRepo<SchemaHistory, i64> RepoSchemaHistory{get;set;}
	[Impl]
	public IList<str> SqlsUp{get;} = new List<str>();
	[Impl]
	public IList<str> SqlsDown{get;} = new List<str>();

	public FullInit(
		ISqlCmdMkr SqlCmdMkr
		,TxnWrapper<DbFnCtx> TxnWrapper
		,ITblMgr TblMgr
		,IAppRepo<SchemaHistory, i64> RepoSchemaHistory
	){
		if(RepoSchemaHistory is not SqlRepo<SchemaHistory, i64> SqlRepoSchemaHistory){
			throw new ArgumentException("RepoSchemaHistory must be SqlRepo<SchemaHistory, i64>");
		}
		this.RepoSchemaHistory = RepoSchemaHistory;
		SqlRepoSchemaHistory.DictMapper = SqlHelperDictMapper.Inst;
		this.SqlCmdMkr = SqlCmdMkr;
		this.TxnWrapper = TxnWrapper;
		this.TblMgr = TblMgr;
		SqlUp = TblMgr.SqlMkSchema();
	}

	[Impl]
	public i64 CreatedMs{get;set;} = 1760270025478;
	public str SqlUp = "";
	/// <summary>
	/// version after ran this migration
	/// </summary>
	//public str Version{get;set;}

	public async Task<Func<
		CT, Task<nil>
	>> FnMkSchema(IDbFnCtx? Ctx, CT Ct){
	var SqlCmd = await SqlCmdMkr.MkCmd(Ctx, SqlUp, Ct);
		return async(Ct)=>{
			try{
				await SqlCmd.WithCtx(Ctx).All(Ct);
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
	>> FnInit(IDbFnCtx? Ctx, CT Ct){
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

	public async Task<nil> UpAsy(CT Ct){
		return await TxnWrapper.Wrap(FnInit, Ct);
	}
	public async Task<nil> DownAsy(CT Ct){
		throw new NotImplementedException();
	}
}
