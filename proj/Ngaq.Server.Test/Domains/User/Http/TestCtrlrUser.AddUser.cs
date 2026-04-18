using Ngaq.Core.Shared.User.Models.Req;
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

		R("AddUser_Should_CallSvcAndReturn200", async(o)=>{
			var called = false;
			var svcUser = new FakeSvcUser{
				OnAddUser = (user, req, ct)=>{
					called = true;
					if(req.Email != "add@example.com"){
						throw new Exception("Unexpected add-user email.");
					}
					return Task.FromResult(NIL);
				}
			};
			var ctrlr = new CtrlrOpenUser(svcUser, new FakeSvcToken());
			var ctx = MkHttpCtx();

			var result = await ctrlr.AddUser(new ReqAddUser{
				Email = "add@example.com",
				UniqName = "u_add",
				Password = "pwd"
			}, ctx, CT.None);

			if(!called){
				throw new Exception("ISvcUser.AddUser was not called.");
			}
			await AssertResultNotNull(result);
			return NIL;
		});
	}
}
