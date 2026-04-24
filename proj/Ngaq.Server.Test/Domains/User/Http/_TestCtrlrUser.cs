using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Model.Sys.Po.Password;
using Ngaq.Core.Model.Sys.Po.RefreshToken;
using Ngaq.Core.Models.Sys.Po.Password;
using Ngaq.Core.Shared.User.Models.Req;
using Ngaq.Core.Shared.User.Models.Bo.Device;
using Ngaq.Core.Shared.User.Models.Po.Device;
using Ngaq.Core.Shared.User.Models.Po.RefreshToken;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.User.Svc;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Backend.Db.TswG;
using Ngaq.Server.Domains.User.Dao;
using Ngaq.Server.Domains.User.Svc;
using Ngaq.Server.Http.Domains.User;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;
using Ngaq.Server.Domains.User;
using Ngaq.Core.Shared.User.Models.Resp;
using Ngaq.Server.Db.User;

namespace Ngaq.Server.Test.Domains.User.Http;

public partial class TestCtrlrUser: ITester{
	readonly ISvcUser _svcUser;
	readonly ISvcToken _svcToken;
	readonly DaoUser _daoUser;
	readonly DaoToken _daoToken;
	readonly IRepo<PoUser, IdUser> _repoUser;
	readonly IRepo<PoPassword, IdPassword> _repoPassword;
	readonly IRepo<PoRefreshToken, IdRefreshToken> _repoRefreshToken;

	str _token = "";
	IdClient _clientId = new();
	IdUser? _createdUserId = null;
	IdPassword? _createdPasswordId = null;
	readonly List<IdRefreshToken> _tokenIds = [];

	public TestCtrlrUser(
		ISvcUser svcUser
		,ISvcToken svcToken
		,DaoUser daoUser
		,DaoToken daoToken
		,IRepo<PoUser, IdUser> repoUser
		,IRepo<PoPassword, IdPassword> repoPassword
		,IRepo<PoRefreshToken, IdRefreshToken> repoRefreshToken
	){
		_svcUser = svcUser;
		_svcToken = svcToken;
		_daoUser = daoUser;
		_daoToken = daoToken;
		_repoUser = repoUser;
		_repoPassword = repoPassword;
		_repoRefreshToken = repoRefreshToken;
	}

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

		R("CtrlrUser_Setup", async(o)=>{
			_token = "ut_server_ctrlr_user_" + Guid.NewGuid().ToString("N");
			_clientId = new IdClient();
			_createdUserId = null;
			_createdPasswordId = null;
			_tokenIds.Clear();
			return NIL;
		});
		RegisterLogin(node);
		RegisterAddUser(node);
		RegisterLogout(node);
		RegisterRefreshToken(node);
		R("CtrlrUser_Cleanup", async(o)=>{
			await CleanupData();
			return NIL;
		});
		return node;
	}

	static Task AssertResultNotNull(IResult result){
		if(result is null){
			throw new Exception("Controller returned null IResult.");
		}
		return Task.CompletedTask;
	}

	DefaultHttpContext MkHttpCtx(){
		var ctx = new DefaultHttpContext();
		ctx.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
		ctx.Request.Headers.UserAgent = "Ngaq.Server.Test.Http";
		ctx.Request.Headers["X-Client-Id"] = _clientId.ToString();
		return ctx;
	}

	DefaultHttpContext MkHttpCtxForUser(IdUser userId){
		var ctx = MkHttpCtx();
		var claims = new List<Claim>{
			new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
			new Claim("sub", userId.ToString())
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		ctx.User = new ClaimsPrincipal(identity);
		return ctx;
	}

	CtrlrOpenUser MkCtrlr(){
		return new CtrlrOpenUser(_svcUser, _svcToken);
	}

	IServerUserCtx MkServerUserCtx(){
		return new ServerUserCtx{
			ClientId = _clientId,
			ClientType = EClientType.ApiTool,
			IpAddr = "127.0.0.1",
			UserAgent = "Ngaq.Server.Test.Http"
		};
	}

	ReqAddUser MkReqAddUser(){
		return new ReqAddUser{
			Email = $"{_token}@example.com",
			UniqName = $"{_token}_user",
			Password = "P@ssw0rd_123456"
		};
	}

	async Task<IdUser> EnsureUserCreated(CT Ct){
		if(_createdUserId is not null){
			return _createdUserId.Value;
		}

		var req = MkReqAddUser();
		var dbCtx = new DbFnCtx();
		var poUser = await _daoUser.SelectByEmail(dbCtx, req.Email, Ct);
		if(poUser is null){
			var ctx = MkHttpCtx();
			var ctrlr = MkCtrlr();
			await ctrlr.AddUser(req, ctx, Ct);
			poUser = await _daoUser.SelectByEmail(dbCtx, req.Email, Ct);
		}
		if(poUser is null){
			throw new Exception("Expected user row after AddUser controller call.");
		}
		_createdUserId = poUser.Id;

		var pwd = await _daoUser.SlctPasswordByUserId(dbCtx, poUser.Id, Ct);
		if(pwd is not null){
			_createdPasswordId = pwd.Id;
		}
		return poUser.Id;
	}

	async Task<RespLogin> LoginByService(CT Ct){
		var req = MkReqAddUser();
		await EnsureUserCreated(Ct);
		var userCtx = MkServerUserCtx();
		var resp = await _svcUser.Login(userCtx, new ReqLogin{
			Email = req.Email,
			Password = req.Password,
			UserIdentityMode = ReqLogin.EUserIdentityMode.Email
		}, Ct);

		var dbCtx = new DbFnCtx();
		var tokens = await _daoToken.SlctValidTokens(dbCtx, IdUser.Parse(resp.UserId), _clientId, Ct);
		await foreach(var token in tokens){
			_tokenIds.Add(token.Id);
		}
		return resp;
	}

	async Task CleanupData(){
		var ctx = new DbFnCtx();

		if(_tokenIds.Count > 0){
			await _repoRefreshToken.BatHardDelById(ctx, AsyE(_tokenIds.Distinct().ToArray()), CT.None);
		}
		if(_createdPasswordId is not null){
			await _repoPassword.BatHardDelById(ctx, AsyE(_createdPasswordId.Value), CT.None);
		}
		if(_createdUserId is not null){
			await _repoUser.BatHardDelById(ctx, AsyE(_createdUserId.Value), CT.None);
		}
	}

	static async IAsyncEnumerable<T> AsyE<T>(params T[] items){
		foreach(var i in items){
			yield return i;
		}
	}
}
