using System.Buffers;
using System.Text;
using Ngaq.Core.Tools;

namespace Ngaq.Web;

public static class ExtnHttpCtx{
	public static HttpContext Body<T>(this HttpContext z, T Body){
		var json = JSON.stringify(Body);
		z.Response.ContentType = "application/json;charset=utf-8";
		z.Response.BodyWriter.Write(Encoding.UTF8.GetBytes(json));
		return z;
	}
}
