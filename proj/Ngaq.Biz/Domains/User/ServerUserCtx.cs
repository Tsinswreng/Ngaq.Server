namespace Ngaq.Biz.Domains.User;

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Shared.User.Models.Bo.Device;
using Ngaq.Core.Shared.User.Models.Po.Device;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Core.Tools;

public class ServerUserCtx : IServerUserCtx{
	public IDictionary<str, obj?>? Props{get;set;}
	public str? IpAddr{get;set;}
	public IdClient? ClientId{get;set;}
	public str? UserAgent{get;set;}
	public EClientType ClientType{get;set;} = EClientType.Unknown;
	public IdUser _UserId = IdUser.Zero;
	public IdUser UserId{get{
		if(_UserId.IsNullOrDefault()){
			throw ItemsErr.User.AuthenticationFailed.ToErr();
		}
		return _UserId;
	}set{
		_UserId = value;
	}}
}


public static class ExtnUserCtx{
	public static IServerUserCtx AsServerUserCtx(
		this IUserCtx z
	){
		if(z is IServerUserCtx R){
			return R;
		}
		throw new InvalidCastException();
	}
}

