using System;
using System.ComponentModel.DataAnnotations;

namespace JinCreek.Server.Admin
{
    /// <summary>
    /// valueがGUIDか"none"なら可とする
    /// </summary>
    public class GuidOrNoneAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value == null || (string)value == "none" || Guid.TryParse((string)value, out var _);
        }
    }
}
