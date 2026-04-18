using Ngaq.Core.Shared.User.Models.Req;
using Ngaq.Core.Shared.User.Models.Resp;
using Ngaq.Core.Shared.User.Models.Po.User;
using Tsinswreng.CsSql;
using Ngaq.Backend.Db.TswG;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Server.Test.Domains.User.Svc;

public partial class TestISvcUser{
	public void RegisterAddLoginLogout(ITestNode node){
		var register = node.MkTestFnRegister(
			typeof(TestISvcUser),
			[typeof(Ngaq.Core.Shared.User.Svc.ISvcUser)],
			[
				nameof(Ngaq.Core.Shared.User.Svc.ISvcUser.AddUser),
				nameof(Ngaq.Core.Shared.User.Svc.ISvcUser.Login),
				nameof(Ngaq.Core.Shared.User.Svc.ISvcUser.Logout)
			],
			nameof(TestISvcUser)
		);
		var R = register.Register;

		R("AddUser_LoginByEmail_Logout_Should_Work", async(o)=>{
			var email = $"{_token}@example.com";
			var uniqName = $"{_token}_user";
			var password = "P@ssw0rd_123456";

			var userCtx = MkServerUserCtx();

			await _svcUser.AddUser(
				userCtx,
				new ReqAddUser{
					Email = email,
					UniqName = uniqName,
					Password = password
				},
				CT.None
			);

			var loginResp = await _svcUser.Login(
				userCtx,
				new ReqLogin{
					Email = email,
					Password = password,
					UserIdentityMode = ReqLogin.EUserIdentityMode.Email
				},
				CT.None
			);

			AssertLoginResp(loginResp, email);
			var userId = loginResp.PoUser!.Id;
			_createdUserId = userId;

			var dbCtx = new DbFnCtx();
			var slctPwd = await _daoUser.FnSlctPasswordByUserId(dbCtx, CT.None);
			var pwd = await slctPwd(userId, CT.None);
			if(pwd is null){
				throw new Exception("Login succeeded but password row not found.");
			}
			_createdPasswordId = pwd.Id;

			var slctTokens = await _daoToken.FnSlctValidTokens(dbCtx, CT.None);
			var tokensBeforeLogout = await ToList(slctTokens(userId, _clientId, CT.None));
			if(tokensBeforeLogout.Count == 0){
				throw new Exception("Expected at least one refresh token after login.");
			}
			_tokenIds.AddRange(tokensBeforeLogout.Select(x=>x.Id));

			return NIL;
		});
	}

	static void AssertLoginResp(RespLogin resp, str expectedEmail){
		if(string.IsNullOrWhiteSpace(resp.AccessToken)){
			throw new Exception("AccessToken should not be empty.");
		}
		if(string.IsNullOrWhiteSpace(resp.RefreshToken)){
			throw new Exception("RefreshToken should not be empty.");
		}
		if(string.IsNullOrWhiteSpace(resp.UserId)){
			throw new Exception("UserId should not be empty.");
		}
		if(resp.PoUser is null){
			throw new Exception("PoUser should not be null.");
		}
		if(!string.Equals(resp.PoUser.Email, expectedEmail, StringComparison.OrdinalIgnoreCase)){
			throw new Exception($"Expected email [{expectedEmail}], got [{resp.PoUser.Email}].");
		}
	}
}
