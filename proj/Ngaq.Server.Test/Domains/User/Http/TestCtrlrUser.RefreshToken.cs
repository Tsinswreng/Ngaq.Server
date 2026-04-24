using Ngaq.Server.Http.Domains.User;
using Ngaq.Core.Shared.User.Models.Req;
using Tsinswreng.CsTreeTest;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Infra.IF;

namespace Ngaq.Server.Test.Domains.User.Http;

public partial class TestCtrlrUser{
	public void RegisterRefreshToken(ITestNode node){
		var register = node.MkTestFnRegister(
			typeof(TestCtrlrUser),
			[typeof(CtrlrOpenUser)],
			[nameof(CtrlrOpenUser.RefreshToken)],
			nameof(TestCtrlrUser)
		);
		var R = register.Register;

		R("RefreshToken_Should_UseRealSvcAndReturn200", async(o)=>{
			var ctrlr = MkCtrlr();
			var loginResp = await LoginByService(CT.None);
			var userId = IdUser.Parse(loginResp.UserId);
			var ctx = MkHttpCtxForUser(userId);

			var result = await ctrlr.RefreshToken(new ReqRefreshTheToken{
				RefreshToken = loginResp.RefreshToken
			}, ctx, CT.None);
			await AssertResultNotNull(result);
			return NIL;
		});
	}
}
