namespace Ngaq.Biz.Domains.User.Dto;

using System.Security.Claims;
using Ngaq.Core.Shared.Base.Models.Req;
using Ngaq.Core.Shared.Base.Models.Resp;
using Ngaq.Core.Shared.User.Models.Po.User;

public partial class RespValidateAccessToken:BaseResp{
	public ClaimsPrincipal? ClaimsPrincipal{get;set;}
}

