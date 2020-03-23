using System.Diagnostics.CodeAnalysis;

namespace JinCreek.Server.Admin
{

    /// <seealso cref="https://github.com/dotnet/corefx/blob/release/3.1/src/System.ComponentModel.Annotations/src/Resources/Strings.resx"/>
    [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class Messages
    {
        /// <summary>
        /// Organization.IsValidがtrueでない
        /// Status Code: 400
        /// </summary>
        public const string InvalidOrganization = "invalid organization";

        /// <summary>
        /// SIMのCSVインポートでSIM IDがない。
        /// 新規登録のときにはSIM IDが空である必要があるのでSIM IDを[Required]にできない。
        /// そのために独自チェックにしている。
        /// Status Code: 400
        /// </summary>
        public const string InvalidSimId = "SimId is null";

        /// <summary>
        /// Status Code: 400
        /// </summary>
        public const string OutOfDateOrganization = "organization out of date";

        /// <summary>
        /// Status Code: 400
        /// </summary>
        public const string OutOfDateUser = "user out of date";

        /// <summary>
        /// Status Code: 400
        /// </summary>
        public const string InvalidAccessToken = "invalid accessToken";

        /// <summary>
        /// 組織コードか、ドメイン名か、ユーザー名か、パスワードが違う
        /// Status Code: 400
        /// </summary>
        public const string InvalidUserNameOrPassword = "Invalid user name or password";

        /// <summary>
        /// ログインユーザの組織と別の組織にアクセスしようとした
        /// Status Code: 403
        /// </summary>
        public const string InvalidRole = "forbidden";

        /// <summary>
        /// Status Code: 400
        /// </summary>
        public const string NotFound = "not found";

        /// <summary>
        /// Status Code: 400
        /// </summary>
        public const string OutOfDate = "Outside the use period";

        /// <summary>
        /// キー重複
        /// Status Code: 400
        /// </summary>
        public const string Duplicate = "Duplicate";

        /// <summary>
        /// 子エンティティのあるエンティティを削除しようとした
        /// see https://docs.microsoft.com/ja-jp/ef/core/saving/cascade-delete
        /// Status Code: 400
        /// </summary>
        public const string ChildEntityExists = "child entity exists";

        /// <summary>
        /// IPアドレスの形式が正しくない
        /// Status Code: 400
        /// </summary>
        public const string InvalidIpAddress = "The field is not a valid IP address.";

        /// <summary>
        /// CIDRの形式が正しくない
        /// Status Code: 400
        /// </summary>
        public const string InvalidCIDR = "The field is not a valid CIDR.";

        /// <summary>
        /// Status Code: 400
        /// </summary>
        public const string InvalidEndDate = "endDate before the startDate";

        /// <summary>
        /// APNにASCII以外の文字が入力された
        /// Status Code: 400
        /// </summary>
        public const string ApnIsOnlyAscii = "Apn_is_only_ASCII";

        /// <summary>
        /// IsolatedNw1IpPoolにASCII以外の文字が入力された
        /// Status Code: 400
        /// </summary>
        public const string IsolatedNw1IpPoolIsOnlyAscii = "IsolatedNw1IpPool_is_only_ASCII";
    }
}
