namespace Ngaq.Biz.Domains.User.Dto;

using Ngaq.Core.Shared.Base.Models.Req;


public partial class ReqValidateAccessToken:BaseReq{
	public string AccessToken { get; set; } = "";
}

