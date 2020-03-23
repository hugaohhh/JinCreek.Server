using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace JinCreek.Server.Common.Models
{
    //認証状態
    public abstract class AuthenticationState
    {
        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Key]
        public Guid Id { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        public DateTime Expiration { get; set; }
    }

    //SIM & 端末認証済み 
    public class SimAndDeviceAuthenticated : AuthenticationState
    {
        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        // OneToOneのために存在
        public Guid SimAndDeviceId { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public SimAndDevice SimAndDevice { get; set; }
    }

    //多要素認証済み
    public class MultiFactorAuthenticated : AuthenticationState
    {
        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        // OneToOneのために存在
        public Guid MultiFactorId { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public MultiFactor MultiFactor { get; set; }
    }
}
