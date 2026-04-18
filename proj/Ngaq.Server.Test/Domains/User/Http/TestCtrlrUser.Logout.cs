using Ngaq.Core.Models.Sys.Req;
using Ngaq.Server.Http.Domains.User;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Server.Test.Domains.User.Http;

public partial class TestCtrlrUser{
	public void RegisterLogout(ITestNode node){
		var register = node.MkTestFnRegister(
			typeof(TestCtrlrUser),
			[typeof(CtrlrOpenUser)],
			[nameof(CtrlrOpenUser.Logout)],
			nameof(TestCtrlrUser)
		);
		var R = register.Register;

		R("Logout_Should_CallSvcAndReturn200", async(o)=>{
			var called = false;
			var svcUser = new FakeSvcUser{
				OnLogout = (user, req, ct)=>{
					called = true;
					return Task.FromResult(NIL);
				}
			};
			var ctrlr = new CtrlrOpenUser(svcUser, new FakeSvcToken());
			var ctx = MkHttpCtx();

			var result = await ctrlr.Logout(new ReqLogout(), ctx, CT.None);

			if(!called){
				throw new Exception("ISvcUser.Logout was not called.");
			}
			await AssertResultNotNull(result);
			return NIL;
		});
	}
}
