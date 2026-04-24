using Ngaq.Core.Shared.User.Models.Req;
using Ngaq.Core.Shared.User.Models.Resp;
using Ngaq.Server.Http.Domains.User;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Server.Test.Domains.User.Http;

public partial class TestCtrlrUser{
	public void RegisterLogin(ITestNode node){
		var register = node.MkTestFnRegister(
			typeof(TestCtrlrUser),
			[typeof(CtrlrOpenUser)],
			[nameof(CtrlrOpenUser.Login)],
			nameof(TestCtrlrUser)
		);
		var R = register.Register;

		R("Login_Should_UseRealSvcAndReturn200", async(o)=>{
			var ctrlr = MkCtrlr();
			var ctx = MkHttpCtx();
			var reqAdd = MkReqAddUser();
			await EnsureUserCreated(CT.None);

			var result = await ctrlr.Login(new ReqLogin{
				Email = reqAdd.Email,
				Password = reqAdd.Password,
				UserIdentityMode = ReqLogin.EUserIdentityMode.Email
			}, ctx, CT.None);
			await AssertResultNotNull(result);
			return NIL;
		});
	}
}
