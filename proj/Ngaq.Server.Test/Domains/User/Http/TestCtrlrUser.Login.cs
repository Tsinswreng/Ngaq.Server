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

		R("Login_Should_CallSvcAndReturn200", async(o)=>{
			var called = false;
			var svcUser = new FakeSvcUser{
				OnLogin = (user, req, ct)=>{
					called = true;
					if(req.Email != "test@example.com"){
						throw new Exception("Unexpected login email.");
					}
					return Task.FromResult(new RespLogin{
						AccessToken = "at",
						RefreshToken = "rt",
						UserId = "1"
					});
				}
			};
			var svcToken = new FakeSvcToken();
			var ctrlr = new CtrlrOpenUser(svcUser, svcToken);
			var ctx = MkHttpCtx();

			var result = await ctrlr.Login(new ReqLogin{
				Email = "test@example.com",
				Password = "pwd",
				UserIdentityMode = ReqLogin.EUserIdentityMode.Email
			}, ctx, CT.None);

			if(!called){
				throw new Exception("ISvcUser.Login was not called.");
			}
			await AssertResultNotNull(result);
			return NIL;
		});
	}
}
