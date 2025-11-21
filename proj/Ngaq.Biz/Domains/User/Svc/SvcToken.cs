namespace Ngaq.Biz.Domains.User.Svc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Ngaq.Biz.Infra.Cfg;
using Ngaq.Core.Shared.User.Models.Po.RefreshToken;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Core.Model.Sys.Po.RefreshToken;
using Ngaq.Local.Db.TswG;
using Tsinswreng.CsCfg;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.Base.Models.Resp;
using Ngaq.Core.Shared.Base.Models.Req;
using Ngaq.Core.Shared.User.Models.Po.Device;
using Ngaq.Core.Shared.User.Models.Bo.Jwt;
using Ngaq.Biz.Domains.User.Dao;
using Ngaq.Biz.Domains.User.Dto;
using Microsoft.Extensions.Caching.Distributed;
using Ngaq.Core.Shared.User.Models.Po.User;
using Tsinswreng.CsSqlHelper;
using Ngaq.Core.Shared.User.Models.Resp;
using Ngaq.Core.Infra.Errors;
using Tsinswreng.CsErr;

public class RespGenJwtToken:BaseResp{
	public Tempus ExpireAt{get;set;}
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
	IAppRepo<PoRefreshToken, IdRefreshToken> RepoToken;
	DaoToken DaoToken;
	IDistributedCache Cache;
	TxnWrapper<DbFnCtx> TxnWrapper;
	public SvcToken(
		ICfgAccessor Cfg
		,IAppRepo<PoRefreshToken, IdRefreshToken> RepoToken
		,DaoToken DaoToken
		,IDistributedCache Cache
		,TxnWrapper<DbFnCtx> TxnWrapper
	){
		this.Cfg = Cfg;
		this.RepoToken = RepoToken;
		this.DaoToken = DaoToken;
		this.Cache = Cache;
		this.TxnWrapper = TxnWrapper;
	}
	public RespGenAccessToken GenAccessToken(
		ReqGenAccessToken Req
	){
		var R = new RespGenAccessToken();
		var JwtSecret = Cfg.Get(ItemsServerCfg.Auth.JwtSecret);
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

		var expires = DateTime.UtcNow.AddMilliseconds(Cfg.Get(ItemsServerCfg.Auth.AccessTokenExpiryMs));
		var token = new JwtSecurityToken(
			issuer: Cfg.Get(ItemsServerCfg.Auth.JwtIssuer)
			,audience: Cfg.Get(ItemsServerCfg.Auth.JwtAudience)
			,claims: claims
			,expires: expires
			,signingCredentials: credentials
		);
		var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
		R.AccessToken = accessToken;
		R.ExpireAt = Tempus.FromDateTime(expires);

		return R;
	}

	public RespGenRefreshToken GenRefreshToken(
		ReqGenRefreshToken Req
	){
		var R = new RespGenRefreshToken();
		var jwtSecret = ItemsServerCfg.Auth.JwtSecret.GetFrom(Cfg);
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

		var expires = now.AddMilliseconds(Cfg.Get(ItemsServerCfg.Auth.RefreshTokenExpiryMs));
		var token = new JwtSecurityToken(
			issuer: Cfg.Get(ItemsServerCfg.Auth.JwtIssuer),
			audience: Cfg.Get(ItemsServerCfg.Auth.JwtAudience),
			claims: claims,
			expires: expires.UtcDateTime,  // 7 天過期
			signingCredentials: credentials
		);
		R.RefreshToken = new JwtSecurityTokenHandler().WriteToken(token);
		R.ExpireAt = Tempus.FromDateTime(expires.UtcDateTime);

		return R;
	}


	public async Task<IAnswer<RespValidateAccessToken>> ValidateAccessTokenAsy(
		ReqValidateAccessToken Req, CT Ct
	){
		var rawAccessToken = Req.AccessToken;
		var R = new Answer<RespValidateAccessToken>();
		R.Ok = true;
		if (string.IsNullOrWhiteSpace(rawAccessToken)){
			return R.AddErr(ItemsErr.User.InvalidToken.ToErr());
		}
		var jwtSecret = Cfg.Get(ItemsServerCfg.Auth.JwtSecret);
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret ?? ""));
		var handler = new JwtSecurityTokenHandler();
		try{
			var validations = new TokenValidationParameters{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = Cfg.Get(ItemsServerCfg.Auth.JwtIssuer),
				ValidAudience = Cfg.Get(ItemsServerCfg.Auth.JwtAudience),
				IssuerSigningKey = key,
				ClockSkew = TimeSpan.FromSeconds(30)//留30s容錯
			};

			var principal = handler.ValidateToken(rawAccessToken, validations, out var securityToken);

			return R.OkWith(new RespValidateAccessToken{
				ClaimsPrincipal = principal
			});
		}
		catch (SecurityTokenExpiredException){
			return R.AddErr(ItemsErr.User.TokenExpired.ToErr());
		}
		catch (SecurityTokenInvalidSignatureException){
			return R.AddErr(ItemsErr.User.InvalidToken.ToErr());
			//return R.AddErrStr("Invalid token signature.");
		}
		catch (Exception ex){
			//return R.AddErrStr($"Token validation failed: {ex.Message}");
			return R.AddErr(ItemsErr.User.InvalidToken.ToErr());
		}
	}

	/// <summary>
	/// AI曰不用做緩存
	/// </summary>
	/// <param name="Ctx"></param>
	/// <param name="Ct"></param>
	/// <returns></returns>
	public async Task<Func<
		IUserCtx
		,str//RawRefreshTokenValue
		,CT, Task<IAnswer<RespRefreshBothToken>>
	>> FnValidateEtRefreshTheToken(IDbFnCtx Ctx, CT Ct){
		var SlctToken = await DaoToken.FnSlctByTokenValue(Ctx, Ct);//TODO 製Svc層之接口、未必從DB讀令牌
		var GenRTokenEtStore = await FnGenEtStoreRefreshToken(Ctx, Ct);
		return async(User, RawTokenStr, Ct)=>{
			var R = new Answer<RespRefreshBothToken>();
			var PoRefreshToken = new PoRefreshToken();
			PoRefreshToken.SetTokenValueSha256(RawTokenStr);
			var OldToken = await SlctToken(PoRefreshToken.TokenValue!, Ct);
			if(OldToken is null){
				R.AddErr(ItemsErr.User.InvalidToken.ToErr());
				return R;
			}
			User.UserId = OldToken.UserId;
			var RespNeoRToken = await GenRTokenEtStore(User, Ct);
			OldToken.RevokeAt = new Tempus();
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
		};
	}

	public async Task<Func<
		IUserCtx
		,CT, Task<RespGenRefreshToken>
	>> FnGenEtStoreRefreshToken(IDbFnCtx Ctx, CT Ct){
		var Insert = await RepoToken.FnInsertOne(Ctx, Ct);
		return async (User, Ct)=>{
			var UserCtx = User.AsServerUserCtx();
			var UserIdStr = UserCtx.UserId.ToString();
			var Resp = GenRefreshToken(new ReqGenRefreshToken{
				UserId = UserIdStr
			});

			var Session = new PoRefreshToken();{
				var o = Session;
				o.UserId = UserCtx.UserId;
				o.BizCreatedAt = new Tempus();
				o.ExpireAt = Resp.ExpireAt;
				o.SetTokenValueSha256(Resp.RefreshToken);
				o.ClientId = UserCtx.ClientId??IdClient.Zero;
				o.ClientType = UserCtx.ClientType;
				o.IpAddr = UserCtx.IpAddr;
				o.UserAgent = UserCtx.UserAgent;
			}
			await Insert(Session, Ct);
			return Resp;
		};
	}

	public async Task<Func<
		PoRefreshToken
		,CT, Task<nil>
	>> FnRevokeRefreshToken(IDbFnCtx Ctx, CT Ct){
		var SlctById = await RepoToken.FnSlctOneById(Ctx, Ct);
		var UpdById = await RepoToken.FnUpdOneById(
			Ctx,[
				nameof(PoRefreshToken.RevokeAt)
				,nameof(PoRefreshToken.BizUpdatedAt)
				,nameof(PoRefreshToken.RevokeReason)
			],Ct
		);
		return async(Po, Ct)=>{
			await UpdById(Po, Ct);
			return NIL;
		};
	}


	public async Task<Func<
		IUserCtx, PoRefreshToken
		,CT, Task<nil>
	>> FnRevokeUsersAllRefreshToken(IDbFnCtx Ctx, CT Ct){
		return async(User, Po, Ct)=>{
			throw new NotImplementedException();
			//return NIL;
		};
	}


	#region TxApi
	public async Task<IAnswer<RespRefreshBothToken>> ValidateEtRefreshTheToken(
		IUserCtx User, str RefreshToken, CT Ct
	){
		return await TxnWrapper.Wrap(FnValidateEtRefreshTheToken, User, RefreshToken, Ct);
	}
	#endregion TxApi

}
