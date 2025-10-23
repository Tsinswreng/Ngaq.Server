namespace Ngaq.Biz.Domains.User;

using Ngaq.Core.Shared.User.Models.Bo.Device;
using Ngaq.Core.Shared.User.Models.Po.Device;
using Ngaq.Core.Shared.User.UserCtx;

public class ServerUserCtx : UserCtx, IServerUserCtx{
	public str? IpAddr{get;set;}
	public IdClient? ClientId{get;set;}
	public str? UserAgent{get;set;}
	public EClientType ClientType{get;set;} = EClientType.Unknown;
}

public static class ExtnUseCtx{
	public static IServerUserCtx AsServerUserCtx(
		this IUserCtx z
	){
		if(z is IServerUserCtx R){
			return R;
		}
		throw new InvalidCastException();
	}
}
