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
        public DateTime TimeLimit { get; set; }
    }

    //SIM & 端末認証済み 
    public class SimDeviceAuthenticationStateDone : AuthenticationState
    {
        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        public Guid SimDeviceId { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public SimDevice SimDevice { get; set; }
    }

    //多要素認証済み
    public class MultiFactorAuthenticationStateDone : AuthenticationState
    {
        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        public Guid FactorCombinationId { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public FactorCombination FactorCombination { get; set; }
    }
}
