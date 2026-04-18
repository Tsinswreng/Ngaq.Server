using Microsoft.AspNetCore.Http;
using Ngaq.Core.Models.Sys.Req;
using Ngaq.Core.Shared.Base.Models.Resp;
using Ngaq.Core.Shared.User.Models.Req;
using Ngaq.Core.Shared.User.Models.Resp;
using Ngaq.Core.Shared.User.Svc;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Server.Domains.User.Dto;
using Ngaq.Server.Domains.User.Svc;
using Ngaq.Server.Http.Domains.User;
using Tsinswreng.CsErr;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Server.Test.Domains.User.Http;

public partial class TestCtrlrUser: ITester{
	public ITestNode RegisterTestsInto(ITestNode? node){
		node ??= new TestNode();
		node.Ordered = true;

		var register = node.MkTestFnRegister(
			typeof(TestCtrlrUser),
			[typeof(CtrlrOpenUser)],
			[],
			nameof(TestCtrlrUser)
		);
		var R = register.Register;

		R("CtrlrUser_Setup", async(o)=>NIL);
		RegisterLogin(node);
		RegisterAddUser(node);
		RegisterLogout(node);
		RegisterRefreshToken(node);
		R("CtrlrUser_Cleanup", async(o)=>NIL);
		return node;
	}

	static Task AssertResultNotNull(IResult result){
		if(result is null){
			throw new Exception("Controller returned null IResult.");
		}
		return Task.CompletedTask;
	}

	static DefaultHttpContext MkHttpCtx(){
		var ctx = new DefaultHttpContext();
		ctx.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
		ctx.Request.Headers.UserAgent = "Ngaq.Server.Test.Http";
		return ctx;
	}

	sealed class FakeSvcUser: ISvcUser{
		public Func<IUserCtx, ReqAddUser, CT, Task<nil>> OnAddUser = (_, _, _)=>Task.FromResult(NIL);
		public Func<IUserCtx, ReqLogin, CT, Task<RespLogin>> OnLogin = (_, _, _)=>Task.FromResult(new RespLogin());
		public Func<IUserCtx, ReqLogout, CT, Task<nil>> OnLogout = (_, _, _)=>Task.FromResult(NIL);

		public Task<nil> AddUser(IUserCtx user, ReqAddUser reqAddUser, CT ct){
			return OnAddUser(user, reqAddUser, ct);
		}
		public Task<RespLogin> Login(IUserCtx user, ReqLogin reqLogin, CT ct){
			return OnLogin(user, reqLogin, ct);
		}
		public Task<nil> Logout(IUserCtx user, ReqLogout reqLogout, CT ct){
			return OnLogout(user, reqLogout, ct);
		}
	}

	sealed class FakeSvcToken: ISvcToken{
		public Func<ReqValidateAccessToken, CT, Task<IAnswer<RespValidateAccessToken>>> OnValidateAccessToken =
			(_, _)=>Task.FromResult<IAnswer<RespValidateAccessToken>>(new Answer<RespValidateAccessToken>().OkWith(new RespValidateAccessToken()));

		public Func<IUserCtx, str, CT, Task<IAnswer<RespRefreshBothToken>>> OnRefreshToken =
			(_, _, _)=>Task.FromResult<IAnswer<RespRefreshBothToken>>(new Answer<RespRefreshBothToken>{
				Ok = true,
				Data = new RespRefreshBothToken()
			});

		public RespGenAccessToken GenAccessToken(ReqGenAccessToken req){
			return new RespGenAccessToken{
				AccessToken = "fake-access-token",
				ExpireAt = new Tsinswreng.CsTempus.Tempus()
			};
		}

		public Task<IAnswer<RespValidateAccessToken>> ValidateAccessToken(ReqValidateAccessToken req, CT ct){
			return OnValidateAccessToken(req, ct);
		}

		public Task<IAnswer<RespRefreshBothToken>> ValidateEtRefreshTheToken(IUserCtx user, str refreshToken, CT ct){
			return OnRefreshToken(user, refreshToken, ct);
		}

		public Task<Func<IUserCtx, CT, Task<RespGenRefreshToken>>> FnGenEtStoreRefreshToken(IDbFnCtx ctx, CT ct){
			return Task.FromResult<Func<IUserCtx, CT, Task<RespGenRefreshToken>>>((_, _)=>Task.FromResult(new RespGenRefreshToken{
				RefreshToken = "fake-refresh-token",
				ExpireAt = new Tsinswreng.CsTempus.Tempus()
			}));
		}

		public Task<Func<IUserCtx, CT, Task<nil>>> FnRevokeTokensForLogout(IDbFnCtx ctx, CT ct){
			return Task.FromResult<Func<IUserCtx, CT, Task<nil>>>((_, _)=>Task.FromResult(NIL));
		}
	}
}
