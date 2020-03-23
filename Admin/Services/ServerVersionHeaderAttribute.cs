using Microsoft.AspNetCore.Mvc.Filters;
using System.IO;
using System.Reflection;

namespace JinCreek.Server.Admin.Services
{
    /// <summary>
    /// レスポンスヘッダにX-Server-Versionを追加する。
    /// その値は埋め込みリソースのVersion.txtから読み込む。
    /// Version.txtにはプリビルドイベントでgit rev-parse HEADが書き込まれる。
    ///
    /// レスポンスヘッダの例：
    /// HTTP/1.1 200 OK
    /// Content-Length: 530
    /// Content-Type: application/json; charset=utf-8
    /// Date: Wed, 04 Mar 2020 02:05:32 GMT
    /// Server: Kestrel
    /// X-Server-Version: 86fa9ad276e401dd3d6e4b8d29b31cec9804af25
    /// </summary>
    public class ServerVersionHeaderAttribute : ActionFilterAttribute
    {
        private readonly string _version;

        public ServerVersionHeaderAttribute()
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("JinCreek.Server.Admin.Version.txt");
            if (stream == null) return;
            using var reader = new StreamReader(stream);
            _version = reader.ReadLine();
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            context.HttpContext.Response.Headers.Add("X-Server-Version", _version);
        }
    }
}
