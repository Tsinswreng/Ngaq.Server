using Ngaq.Core.Shared.User.Models.Req;
using Ngaq.Core.Tools;
using Ngaq.Server.Http.Domains.User;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Server.Test.Domains.User.Http;

public partial class TestCtrlrUser{
	public void RegisterAddUser(ITestNode node){
		var register = node.MkTestFnRegister(
			typeof(TestCtrlrUser),
			[typeof(CtrlrOpenUser)],
			[nameof(CtrlrOpenUser.AddUser)],
			nameof(TestCtrlrUser)
		);
		var R = register.Register;

		R("AddUser_Should_UseRealSvcAndReturn200", async(o)=>{
			var ctrlr = MkCtrlr();
			var ctx = MkHttpCtx();
			var uniq = "ut_ctrlr_add_" + Guid.NewGuid().ToString("N");
			var req = new ReqAddUser{
				Email = $"{uniq}@example.com",
				UniqName = $"{uniq}_user",
				Password = "P@ssw0rd_123456"
			};

			var result = await ctrlr.AddUser(req, ctx, CT.None);

			var dbCtx = new DbFnCtx();
			var poUser = await _daoUser.SelectByEmail(dbCtx, req.Email, CT.None);
			if(poUser is null){
				throw new Exception("Expected user row after AddUser.");
			}
			_createdUserId = poUser.Id;
			var pwd = await _daoUser.SlctPasswordByUserId(dbCtx, poUser.Id, CT.None);
			if(pwd is not null){
				_createdPasswordId = pwd.Id;
			}
			await AssertResultNotNull(result);
			return NIL;
		});
	}
}
