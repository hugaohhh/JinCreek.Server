using System.Diagnostics.CodeAnalysis;

namespace JinCreek.Server.Common.Models
{
    public class Radusergroup
    {
        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public uint Id { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public string Username { get; set; }
        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public string Groupname { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public int Priority { get; set; }
    }
}
