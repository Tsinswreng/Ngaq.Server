namespace Ngaq.Biz.Domains.User;

using Ngaq.Core.Shared.User.Models.Bo.Device;
using Ngaq.Core.Shared.User.Models.Po.Device;
using Ngaq.Core.Shared.User.UserCtx;



public interface IServerUserCtx:IUserCtx{
	public str? IpAddr{get;set;}
	public IdClient? ClientId{get;set;}
	public str? UserAgent{get;set;}
	public EClientType ClientType{get;set;}
}
