using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace JinCreek.Server.Common.Models
{
    //認証操作
    public abstract class AuthenticationLog
    {
        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Key]
        public Guid Id { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        public DateTime ConnectionTime { get; set; }

    }

    //多要素認証
    public class MultiFactorAuthenticationLogSuccess : AuthenticationLog
    {
        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        public Guid FactorCombinationId { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public FactorCombination FactorCombination { get; set; }
    }

    //認証解除
    public class Deauthentication: AuthenticationLog
    {
        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        public Guid FactorCombinationId { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public FactorCombination FactorCombination { get; set; }
    }

    //SIM & 端末認証
    public class SimDeviceAuthenticationLogSuccess : AuthenticationLog
    {
        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public SimDevice SimDevice { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        public Guid SimDeviceId { get; set; }
    }

    //SimDevice 認証失敗
    public class SimDeviceAuthenticationLogFail : AuthenticationLog
    {
        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public Sim Sim { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public Guid SimId { get; set; }

    }

    //多要素認証失敗
    public class MultiFactorAuthenticationLogFail : AuthenticationLog
    {
        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public SimDevice SimDevice { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public Guid SimDeviceId { get; set; }
    }

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
}
