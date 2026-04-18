using Ngaq.Server.Http.Domains.User;
using Ngaq.Server.Domains.User.Dto;
using Ngaq.Core.Shared.User.Models.Req;
using Ngaq.Core.Shared.User.Models.Resp;
using Tsinswreng.CsErr;
using Tsinswreng.CsTreeTest;

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

		R("RefreshToken_Should_CallSvcAndReturn200", async(o)=>{
			var called = false;
			var svcToken = new FakeSvcToken{
				OnRefreshToken = (user, refreshToken, ct)=>{
					called = true;
					if(refreshToken != "rt-old"){
						throw new Exception("Unexpected refresh token.");
					}
					var ans = new Answer<RespRefreshBothToken>{
						Ok = true,
						Data = new RespRefreshBothToken{
						AccessToken = "new-at",
						RefreshToken = "new-rt"
						}
					};
					return Task.FromResult<IAnswer<RespRefreshBothToken>>(ans);
				}
			};
			var ctrlr = new CtrlrOpenUser(new FakeSvcUser(), svcToken);
			var ctx = MkHttpCtx();

			var result = await ctrlr.RefreshToken(new ReqRefreshTheToken{
				RefreshToken = "rt-old"
			}, ctx, CT.None);

			if(!called){
				throw new Exception("ISvcToken.ValidateEtRefreshTheToken was not called.");
			}
			await AssertResultNotNull(result);
			return NIL;
		});
	}
}
