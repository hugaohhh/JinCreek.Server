namespace JinCreek.Server.Admin.CustomProvider
{
    public static class Roles
    {
        /// <summary>
        /// スーパー管理者（管理画面にサインイン可）
        /// </summary>
        public const string SuperAdmin = "SuperAdmin";

        /// <summary>
        /// ユーザー管理者（管理画面にサインイン可）
        /// </summary>
        public const string UserAdmin = "UserAdmin";

        /// <summary>
        /// 一般ユーザー（管理画面にサインイン不可）
        /// </summary>
        public const string GeneralUser = "GeneralUser";
    }
}
