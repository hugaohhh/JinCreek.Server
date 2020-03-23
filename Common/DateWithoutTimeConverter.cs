using Newtonsoft.Json.Converters;

namespace JinCreek.Server.Common
{
    /// <summary>
    /// DateTimeをyyyy-MM-dd形式にシリアライズする
    /// </summary>
    /// <see cref="https://docs.microsoft.com/ja-jp/aspnet/core/web-api/advanced/formatting?view=aspnetcore-3.1"/>
    /// <seealso cref="https://docs.microsoft.com/ja-jp/dotnet/standard/serialization/system-text-json-converters-how-to#registration-sample---jsonconverter-on-a-property"/>
    /// <seealso cref="https://forums.asp.net/t/2121065.aspx?JSON+serilizer+and+date+format"/>
    public class DateWithoutTimeConverter : IsoDateTimeConverter
    {
        public DateWithoutTimeConverter()
        {
            DateTimeFormat = "yyyy'-'MM'-'dd";
        }
    }
}
