using Ngaq.Core.Shared.User.Models.Req;
using Ngaq.Core.Tools;
using Ngaq.Server.Http.Domains.User;
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
			var req = MkReqAddUser();

			var result = await ctrlr.AddUser(req, ctx, CT.None);

			var userId = await EnsureUserCreated(CT.None);
			if(userId.IsNullOrDefault()){
				throw new Exception("Expected non-default user id after AddUser.");
			}
			await AssertResultNotNull(result);
			return NIL;
		});
	}
}
