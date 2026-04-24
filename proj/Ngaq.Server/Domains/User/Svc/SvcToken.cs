namespace Ngaq.Server.Domains.User.Svc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Ngaq.Server.Infra.Cfg;
using Ngaq.Core.Shared.User.Models.Po.RefreshToken;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Core.Model.Sys.Po.RefreshToken;
using Ngaq.Backend.Db.TswG;
using Tsinswreng.CsCfg;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.Base.Models.Resp;
using Ngaq.Core.Shared.Base.Models.Req;
using Ngaq.Core.Shared.User.Models.Po.Device;
using Ngaq.Core.Shared.User.Models.Bo.Jwt;
using Ngaq.Server.Domains.User.Dao;
using Ngaq.Server.Domains.User.Dto;
using Microsoft.Extensions.Caching.Distributed;
using Ngaq.Core.Shared.User.Models.Po.User;
using Tsinswreng.CsSql;
using Ngaq.Core.Shared.User.Models.Resp;
using Ngaq.Core.Infra.Errors;
using Tsinswreng.CsErr;
using Tsinswreng.CsTempus;
using Tsinswreng.CsTools;

public class RespGenJwtToken:BaseResp{
	public UnixMs ExpireAt{get;set;}
	public Jti Jti{get;set;}
}

public class RespGenRefreshToken:RespGenJwtToken{
	public str RefreshToken{get;set;} = "";
}

public class RespGenAccessToken:RespGenJwtToken{
	public str AccessToken{get;set;} = "";
}

public class ReqGenToken:BaseReq{
	public str UserId{get;set;} = "";
}

public class ReqGenRefreshToken:ReqGenToken{}

public class ReqGenAccessToken:ReqGenToken{}

public class SvcToken
	:ISvcToken
{
	ICfgAccessor Cfg;
	IRepo<PoRefreshToken, IdRefreshToken> RepoToken;
	DaoToken DaoToken;
	IDistributedCache Cache;
	ISqlCmdMkr SqlCmdMkr;
	public SvcToken(
		ICfgAccessor Cfg
		, IRepo<PoRefreshToken, IdRefreshToken> RepoToken
		,DaoToken DaoToken
		,IDistributedCache Cache
		,ISqlCmdMkr SqlCmdMkr
	){
		this.Cfg = Cfg;
		this.RepoToken = RepoToken;
		this.DaoToken = DaoToken;
		this.Cache = Cache;
		this.SqlCmdMkr = SqlCmdMkr;
	}
	public RespGenAccessToken GenAccessToken(
		ReqGenAccessToken Req
	){
		var R = new RespGenAccessToken();
		var JwtSecret = Cfg.Get(KeysServerCfg.Auth.JwtSecret);
		var securityKey = new SymmetricSecurityKey(
			//注意: 小於256字節則報錯
			Encoding.UTF8.GetBytes(JwtSecret??"")
		);
		var credentials = new SigningCredentials(
			securityKey, SecurityAlgorithms.HmacSha256Signature
		);
		var Jti = new Jti();

		var claims = new[]{
			//subject, 标识令牌的归属实体（如用户、服务或设备）
			new Claim(JwtRegisteredClaimNames.Sub, Req.UserId)
			//Jwt Id 为令牌提供全局唯一标识符，防范重放攻击（Replay Attack）
			,new Claim(JwtRegisteredClaimNames.Jti, Jti.ToString())
			//issuedAt
			,new Claim(
				JwtRegisteredClaimNames.Iat
				,DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
				,ClaimValueTypes.Integer64
			)
			//,new Claim("role", "admin")//custom
		};

		var expires = DateTime.UtcNow.AddMilliseconds(Cfg.Get(KeysServerCfg.Auth.AccessTokenExpiryMs));
		var token = new JwtSecurityToken(
			issuer: Cfg.Get(KeysServerCfg.Auth.JwtIssuer)
			,audience: Cfg.Get(KeysServerCfg.Auth.JwtAudience)
			,claims: claims
			,expires: expires
			,signingCredentials: credentials
		);
		var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
		R.AccessToken = accessToken;
		R.ExpireAt = UnixMs.FromDateTime(expires);

		return R;
	}

	public RespGenRefreshToken GenRefreshToken(
		ReqGenRefreshToken Req
	){
		var R = new RespGenRefreshToken();
		var jwtSecret = KeysServerCfg.Auth.JwtSecret.GetFrom(Cfg);
		var securityKey = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(jwtSecret ?? "")
		);
		var credentials = new SigningCredentials(
			securityKey, SecurityAlgorithms.HmacSha256Signature
		);

		var now = DateTimeOffset.UtcNow;
		var jti = new Jti();
		R.Jti = jti;
		var claims = new[]
		{
			// 誰的令牌
			new Claim(JwtRegisteredClaimNames.Sub, Req.UserId),
			// 唯一標識（存庫時只存這個哈希）
			new Claim(JwtRegisteredClaimNames.Jti, jti.ToString()),
			// 發放時間
			new Claim(JwtRegisteredClaimNames.Iat,
					now.ToUnixTimeSeconds().ToString(),
					ClaimValueTypes.Integer64),
		};

		var expires = now.AddMilliseconds(Cfg.Get(KeysServerCfg.Auth.RefreshTokenExpiryMs));
		var token = new JwtSecurityToken(
			issuer: Cfg.Get(KeysServerCfg.Auth.JwtIssuer),
			audience: Cfg.Get(KeysServerCfg.Auth.JwtAudience),
			claims: claims,
			expires: expires.UtcDateTime,  // 7 天過期
			signingCredentials: credentials
		);
		R.RefreshToken = new JwtSecurityTokenHandler().WriteToken(token);
		R.ExpireAt = UnixMs.FromDateTime(expires.UtcDateTime);

		return R;
	}


	public async Task<IAnswer<RespValidateAccessToken>> ValidateAccessToken(
		ReqValidateAccessToken Req, CT Ct
	){
		var rawAccessToken = Req.AccessToken;
		var R = new Answer<RespValidateAccessToken>();
		R.Ok = true;
		if (string.IsNullOrWhiteSpace(rawAccessToken)){
			return R.AddErr(KeysErr.User.InvalidToken.ToErr());
		}
		var jwtSecret = Cfg.Get(KeysServerCfg.Auth.JwtSecret);
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret ?? ""));
		var handler = new JwtSecurityTokenHandler();
		try{
			var validations = new TokenValidationParameters{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = Cfg.Get(KeysServerCfg.Auth.JwtIssuer),
				ValidAudience = Cfg.Get(KeysServerCfg.Auth.JwtAudience),
				IssuerSigningKey = key,
				ClockSkew = TimeSpan.FromSeconds(30)//留30s容錯
			};

			var principal = handler.ValidateToken(rawAccessToken, validations, out var securityToken);

			return R.OkWith(new RespValidateAccessToken{
				ClaimsPrincipal = principal
			});
		}
		catch (SecurityTokenExpiredException){
			return R.AddErr(KeysErr.User.TokenExpired.ToErr());
		}
		catch (SecurityTokenInvalidSignatureException){
			return R.AddErr(KeysErr.User.InvalidToken.ToErr());
			//return R.AddErrStr("Invalid token signature.");
		}
		catch (Exception){
			//return R.AddErrStr($"Token validation failed: {ex.Message}");
			return R.AddErr(KeysErr.User.InvalidToken.ToErr());
		}
	}

	
	/// AI曰不用做緩存
	
	/// <param name="Ctx"></param>
	/// <param name="Ct"></param>
	/// <returns></returns>
	protected async Task<IAnswer<RespRefreshBothToken>> ValidateEtRefreshTheTokenInTxn(
		IDbFnCtx Ctx
		,IUserCtx User
		,str RawTokenStr
		,CT Ct
	){
		var R = new Answer<RespRefreshBothToken>();
		var PoRefreshToken = new PoRefreshToken();
		PoRefreshToken.SetTokenValueSha256(RawTokenStr);
		var OldToken = await DaoToken.SlctByTokenValue(Ctx, PoRefreshToken.TokenValue!, Ct);//TODO 製Svc層之接口、未必從DB讀令牌
		if(OldToken is null){
			R.AddErr(KeysErr.User.InvalidToken.ToErr());
			return R;
		}
		User.UserId = OldToken.UserId;
		var RespNeoRToken = await GenEtStoreRefreshToken(Ctx, User, Ct);
		OldToken.RevokeAt = new UnixMs();
		var RespNeoAToken = GenAccessToken(new ReqGenAccessToken{
			UserId = User.UserId.ToString()
		});
		var Resp = new RespRefreshBothToken{
			AccessToken = RespNeoAToken.AccessToken
			,AccessTokenExpireAt = RespNeoAToken.ExpireAt
			,RefreshToken = RespNeoRToken.RefreshToken
			,RefreshTokenExpireAt = RespNeoRToken.ExpireAt
		};
		return R.OkWith(Resp);
	}

	public async Task<RespGenRefreshToken> GenEtStoreRefreshToken(
		IDbFnCtx Ctx
		,IUserCtx User
		,CT Ct
	){
		var UserCtx = User.AsServerUserCtx();
		var UserIdStr = UserCtx.UserId.ToString();
		var Resp = GenRefreshToken(new ReqGenRefreshToken{
			UserId = UserIdStr
		});

		var Session = new PoRefreshToken();{
			var o = Session;
			o.UserId = UserCtx.UserId;
			o.BizCreatedAt = new UnixMs();
			o.ExpireAt = Resp.ExpireAt;
			o.SetTokenValueSha256(Resp.RefreshToken);
			o.ClientId = UserCtx.ClientId;
			o.ClientType = UserCtx.ClientType;
			o.IpAddr = UserCtx.IpAddr;
			o.UserAgent = UserCtx.UserAgent;
		}
		await RepoToken.BatAdd(Ctx, OneAsyE(Session), Ct);
		return Resp;
	}

	protected async Task<nil> RevokeRefreshTokens(
		IDbFnCtx Ctx
		,IAsyncEnumerable<PoRefreshToken> Tokens
		,CT Ct
	){
		// 使用批次收集器按批更新，避免整體載入內存，同時避免同連接讀寫交錯。
		await using var Revoker = new BatchCollector<PoRefreshToken, IRespBatUpd>(
			async(TokenBatch, Ct)=>{
				var Now = UnixMs.Now();
				for(var i = 0; i < TokenBatch.Count; i++){
					var Token = TokenBatch[i];
					Token.RevokeAt = Now;
					Token.BizUpdatedAt = Now;
					Token.RevokeReason = "Logout";
				}
				return await RepoToken.BatUpd(Ctx, ToAsyE(TokenBatch), Ct);
			}
		);
		await Revoker.AddRange(Tokens, null, Ct);
		await Revoker.End(Ct);
		return NIL;
	}


	public async Task<nil> RevokeUsersAllRefreshToken(
		IDbFnCtx Ctx
		,IUserCtx User
		,PoRefreshToken Po
		,CT Ct
	){
		throw new NotImplementedException();
	}

	public async Task<nil> RevokeTokensForLogout(
		IDbFnCtx Ctx
		,IUserCtx User
		,CT Ct
	){
		var SrvUser = User.AsServerUserCtx();
		var ValidTokens = await DaoToken.SlctValidTokens(Ctx, User.UserId, SrvUser.ClientId, Ct);
		await RevokeRefreshTokens(Ctx, ValidTokens, Ct);
		return NIL;
	}

	protected static async IAsyncEnumerable<PoRefreshToken> ToAsyE(IList<PoRefreshToken> Tokens){
		for(var i = 0; i < Tokens.Count; i++){
			yield return Tokens[i];
		}
	}

	protected static async IAsyncEnumerable<PoRefreshToken> OneAsyE(PoRefreshToken Token){
		yield return Token;
	}


	#region TxApi
	public async Task<IAnswer<RespRefreshBothToken>> ValidateEtRefreshTheToken(
		IUserCtx User, str RefreshToken, CT Ct
	){
		return await SqlCmdMkr.RunInTxn(Ct, async(Ctx)=>{
			return await ValidateEtRefreshTheTokenInTxn(Ctx, User, RefreshToken, Ct);
		});
	}
	#endregion TxApi

}
