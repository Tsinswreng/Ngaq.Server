namespace Ngaq.Biz.Svc;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Ngaq.Biz.Infra.Cfg;
using Tsinswreng.CsCfg;



public class SvcToken{

	public str GenAccessToken(
		str UserIdStr
	){
		var JwtSecret = ServerCfgItems.JwtSecret.GetFrom(ServerCfg.Inst);
		var securityKey = new SymmetricSecurityKey(
			//注意: 小於256字節則報錯
			//Encoding.UTF8.GetBytes("2025-04-16T21:00:39.328+08:00_W16-3=2025-04-16T21:00:50.706+08:00_W16-3")
			Encoding.UTF8.GetBytes(JwtSecret??"")
		);
		var credentials = new SigningCredentials(
			securityKey, SecurityAlgorithms.HmacSha256Signature
		);
		var claims = new[]{
			//subject, 标识令牌的归属实体（如用户、服务或设备）
			new Claim(JwtRegisteredClaimNames.Sub, UserIdStr)
			//Jwt Id 为令牌提供全局唯一标识符，防范重放攻击（Replay Attack）
			,new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()??"")
			//issuedAt
			,new Claim(
				JwtRegisteredClaimNames.Iat
				,DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
				,ClaimValueTypes.Integer64
			)
			//,new Claim("role", "admin")//custom
		};
		var token = new JwtSecurityToken(
			issuer: "service-alpha-dev"
			,audience: "client-app-dev"
			,claims: claims
			,expires: DateTime.UtcNow.AddMinutes(30)
			,signingCredentials: credentials
		);
		var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
		return accessToken;
	}

	public str GenRefreshToken(
		str UserIdStr
	){
		var jwtSecret = ServerCfgItems.JwtSecret.GetFrom(ServerCfg.Inst);
		var securityKey = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(jwtSecret ?? "")
		);
		var credentials = new SigningCredentials(
			securityKey, SecurityAlgorithms.HmacSha256Signature
		);

		var now = DateTimeOffset.UtcNow;
		var claims = new[]
		{
			// 誰的令牌
			new Claim(JwtRegisteredClaimNames.Sub, UserIdStr),
			// 唯一標識（存庫時只存這個哈希）
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			// 發放時間
			new Claim(JwtRegisteredClaimNames.Iat,
					now.ToUnixTimeSeconds().ToString(),
					ClaimValueTypes.Integer64),
		};

		var token = new JwtSecurityToken(
			issuer: "service-alpha-dev",
			audience: "client-app-dev",
			claims: claims,
			expires: now.AddDays(7).UtcDateTime,   // 7 天過期
			signingCredentials: credentials
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}


}
