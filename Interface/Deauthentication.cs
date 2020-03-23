﻿#nullable enable
using System.ComponentModel.DataAnnotations;

namespace JinCreek.Server.Interfaces
{
    /// <summary>
    /// 認証解除リクエスト
    /// </summary>
    public class DeauthenticationRequest
    {
        /// <summary>
        /// クライアント証明書の内容(Base64)
        /// </summary>
        /// <example>
        /// "Q2VydGlmaWNhdGU6DQogICAgRGF0YToNCiAgICAgICAgVmVyc2lvbjogMyAoMHgyKQ0KICAgICAgICBTZXJpYWwgTnVtYmVyOiAyICgweDIpDQogICAgICAgIFNpZ25hdHVyZSBBbGdvcml0aG06IHNoYTI1NldpdGhSU0FFbmNyeXB0aW9uDQogICAgICAgIElzc3VlcjogQz1KUCwgU1Q9VG9reW8sIEw9Q2h1by1rdSwgTz1KaW5DcmVlaywgQ049SmluQ3JlZWsgQ0ENCiAgICAgICAgVmFsaWRpdHkNCiAgICAgICAgICAgIE5vdCBCZWZvcmU6IERlYyAyNiAwNzowODo1NyAyMDE5IEdNVA0KICAgICAgICAgICAgTm90IEFmdGVyIDogRGVjIDI1IDA3OjA4OjU3IDIwMjAgR01UDQogICAgICAgIFN1YmplY3Q6IEM9SlAsIFNUPVRva3lvLCBPPUppbkNyZWVrLCBDTj1ZYW1hZGEgVGFybyAxDQogICAgICAgIFN1YmplY3QgUHVibGljIEtleSBJbmZvOg0KICAgICAgICAgICAgUHVibGljIEtleSBBbGdvcml0aG06IHJzYUVuY3J5cHRpb24NCiAgICAgICAgICAgICAgICBSU0EgUHVibGljLUtleTogKDIwNDggYml0KQ0KICAgICAgICAgICAgICAgIE1vZHVsdXM6DQogICAgICAgICAgICAgICAgICAgIDAwOmMyOjZmOjMwOjk0Ojk3OjE4Ojc2OmVlOmI1OmI1OjhlOmMwOmQxOjEzOg0KICAgICAgICAgICAgICAgICAgICA2ZjplZjo4MzozNDpiZTo1NzoyMDozNjpiYzpmMDphZDpiYjozNTpiMDo2OToNCiAgICAgICAgICAgICAgICAgICAgZjk6ODc6NzY6MTk6M2U6YmE6YTc6NTI6N2U6N2E6Mjk6YTY6NGM6Y2M6OWQ6DQogICAgICAgICAgICAgICAgICAgIGM1OjU0OmU0OmYwOjZmOjJiOjM0OjA0OjBiOjYxOjZjOjlhOjZhOjk3OjgwOg0KICAgICAgICAgICAgICAgICAgICAxZDo0ZTpkMTpiODplYzpiMjozNTphOTplZDo4ZjphMDowNzpiNjo4NjozMToNCiAgICAgICAgICAgICAgICAgICAgNWU6YTU6MWU6ODA6OTY6NGM6NTk6MjY6MzY6Nzg6MjQ6YWY6YjU6ZGQ6NmU6DQogICAgICAgICAgICAgICAgICAgIDA4OmM4OjMzOjUzOmM2OmJmOmFjOmEyOjAyOmEzOjM1OjExOmExOmIwOmRkOg0KICAgICAgICAgICAgICAgICAgICA0MDo0MTo0Yzo5ODo0YjphYzphOTo1YzowNzowMzo0Njo3NjpkYzoyYjo5MDoNCiAgICAgICAgICAgICAgICAgICAgMzY6ZmI6Yjc6YjU6MmY6YTU6ODY6YTc6OWQ6NWY6OTA6YWY6MjA6NGM6OWU6DQogICAgICAgICAgICAgICAgICAgIDMyOjhiOjNjOmQ3OjcyOjA2OjA2OjJiOmE3OmYwOjliOmViOjRkOjlmOjViOg0KICAgICAgICAgICAgICAgICAgICBlMDozYjoyNjo0YTo5NTozYjo3MTo0YjphYzozZTphZTo0YzphMTo3NToyNToNCiAgICAgICAgICAgICAgICAgICAgMDM6MTY6ZGU6ZWI6OTQ6NmY6NGY6ZTU6ZTY6Y2Q6MjI6OWE6OTc6ZWQ6YWE6DQogICAgICAgICAgICAgICAgICAgIDk3OjZkOjlkOmNmOmI0OjgzOjQyOmMzOjU5OmZkOjk3Ojc3OjhlOjk4OmVlOg0KICAgICAgICAgICAgICAgICAgICBmNzo2NjoxNzozMDoyZDoyMDo3ZTo5NjowZDowODo1ZDozMzo5YzoyMTo3ZDoNCiAgICAgICAgICAgICAgICAgICAgODk6NTA6MTY6YTg6YWI6YWU6YWE6ZmE6ZWM6Yzg6MGQ6YzQ6NWI6ZTU6Yjk6DQogICAgICAgICAgICAgICAgICAgIDk3OjQ2OmYwOjk5OjVjOjM0OmJlOmU5OjJiOjFiOmMwOjZlOmY5OjQyOjdmOg0KICAgICAgICAgICAgICAgICAgICBhZjo3NTpiYjo1NTpiMTphYjplNjo1NDphMDpkMzo2ZTpmMjo3ODo3MjpiMDoNCiAgICAgICAgICAgICAgICAgICAgNjI6MzUNCiAgICAgICAgICAgICAgICBFeHBvbmVudDogNjU1MzcgKDB4MTAwMDEpDQogICAgICAgIFg1MDl2MyBleHRlbnNpb25zOg0KICAgICAgICAgICAgWDUwOXYzIEJhc2ljIENvbnN0cmFpbnRzOiANCiAgICAgICAgICAgICAgICBDQTpGQUxTRQ0KICAgICAgICAgICAgTmV0c2NhcGUgQ2VydCBUeXBlOiANCiAgICAgICAgICAgICAgICBTU0wgQ2xpZW50LCBTL01JTUUsIE9iamVjdCBTaWduaW5nDQogICAgICAgICAgICBOZXRzY2FwZSBDb21tZW50OiANCiAgICAgICAgICAgICAgICBPcGVuU1NMIEdlbmVyYXRlZCBDZXJ0aWZpY2F0ZQ0KICAgICAgICAgICAgWDUwOXYzIFN1YmplY3QgS2V5IElkZW50aWZpZXI6IA0KICAgICAgICAgICAgICAgIEU2OkU4Ojc4OjlCOkI5OkVGOjM3OjA5OjUwOjBCOkE2OjhDOjIyOjY0OkNCOjkyOjE0OkEwOjI4OkM5DQogICAgICAgICAgICBYNTA5djMgQXV0aG9yaXR5IEtleSBJZGVudGlmaWVyOiANCiAgICAgICAgICAgICAgICBrZXlpZDpFQzpDRjpGMDo4NzoxRToyQTo0NzpEODo3MjoyQzo1RTo1Qjo5Qjo4Qjo5Njo5QjoxQzpBQTo1ODo0MQ0KDQogICAgU2lnbmF0dXJlIEFsZ29yaXRobTogc2hhMjU2V2l0aFJTQUVuY3J5cHRpb24NCiAgICAgICAgIGFjOmMzOjhiOjBlOjk0OmRmOmMzOjE1OjU5OmM0OmE1OjQ2OmRiOmUwOjAzOjBhOmY2OjJhOg0KICAgICAgICAgZWY6NTM6ZWE6YmU6NGU6YmM6YjY6ZTI6MjE6ZmU6ODg6MDY6NmI6OTE6Yjk6ZWM6NDk6NTQ6DQogICAgICAgICBkNjo1MjpkZDoyZjpmZTo1Yjo0NzpmNTo0ODphMTozMjphMzo0ZTpmMzoyZDoyYjpjNzo1NzoNCiAgICAgICAgIDkzOjk5OjcwOmEzOjE1OjFiOjhmOjIwOjE4OmUzOmM1OmY4OjZmOmU4OmNiOjgzOjkwOmExOg0KICAgICAgICAgYjQ6NDM6YjM6ZDM6YjQ6OGQ6ZGE6ODU6M2Q6OWM6MDU6ZmE6NmM6ODU6MDc6YTA6OWE6YTk6DQogICAgICAgICBjZjo2NTo0ODo5Mzo2ZjpiNzoxOTo0MDphMTo5ODowZjpjNjpkNTo1ODpmNDpjYTo2NjozMjoNCiAgICAgICAgIDhjOmUwOjNkOjA3OmY4OmU4OjUzOjBlOjg4OjM5OjZkOmZkOjk4OjAwOjMxOjFlOjQ0OjljOg0KICAgICAgICAgMDM6OGE6N2M6OGE6MTg6N2Y6Y2Q6YmY6ZDE6YjY6YTg6NWQ6Mzk6Yzc6YmU6YTc6YmI6NzE6DQogICAgICAgICA5NzozMTo3NTphMDo5Nzo2Yzo4Mzo1ODo5YTpkYzoyYjo5ODpjZjpmMzpmZjoyODo5ZDo1OToNCiAgICAgICAgIGQzOjI0OjdiOmI5OjJhOmRiOjA5OmRkOjkzOjYyOjczOmJkOjEzOjNmOmMxOmUyOmI2OjFiOg0KICAgICAgICAgMTY6NjQ6ZTM6YWY6Yzg6ODU6MzM6ZTA6OTM6ODU6Nzg6YzI6YjE6MDQ6MzY6MTY6NDQ6YzQ6DQogICAgICAgICBlMDpiMTo3NzpmNTowYToyODpiYjpkNjowYjo3YzphNjpmZDo4OTpiYjplOTo3YToxODo4MToNCiAgICAgICAgIDhkOjFhOmY2OjM3OjhhOmY5OmVlOjNmOmJjOmQxOjBkOjI2OmUyOjExOmFmOjA5OjA1OjRjOg0KICAgICAgICAgMmQ6ZGQ6ZmI6M2M6Y2I6MTY6ODE6YTc6YzM6YmU6YmQ6NTc6M2M6MDY6NDg6Mjg6Mzg6ZWM6DQogICAgICAgICBmMTphNDoxZjo5NQ0KLS0tLS1CRUdJTiBDRVJUSUZJQ0FURS0tLS0tDQpNSUlEcXpDQ0FwT2dBd0lCQWdJQkFqQU5CZ2txaGtpRzl3MEJBUXNGQURCWU1Rc3dDUVlEVlFRR0V3SktVREVPDQpNQXdHQTFVRUNBd0ZWRzlyZVc4eEVEQU9CZ05WQkFjTUIwTm9kVzh0YTNVeEVUQVBCZ05WQkFvTUNFcHBia055DQpaV1ZyTVJRd0VnWURWUVFEREF0S2FXNURjbVZsYXlCRFFUQWVGdzB4T1RFeU1qWXdOekE0TlRkYUZ3MHlNREV5DQpNalV3TnpBNE5UZGFNRWd4Q3pBSkJnTlZCQVlUQWtwUU1RNHdEQVlEVlFRSURBVlViMnQ1YnpFUk1BOEdBMVVFDQpDZ3dJU21sdVEzSmxaV3N4RmpBVUJnTlZCQU1NRFZsaGJXRmtZU0JVWVhKdklERXdnZ0VpTUEwR0NTcUdTSWIzDQpEUUVCQVFVQUE0SUJEd0F3Z2dFS0FvSUJBUURDYnpDVWx4aDI3clcxanNEUkUyL3ZnelMrVnlBMnZQQ3R1eld3DQphZm1IZGhrK3VxZFNmbm9wcGt6TW5jVlU1UEJ2S3pRRUMyRnNtbXFYZ0IxTzBianNzaldwN1krZ0I3YUdNVjZsDQpIb0NXVEZrbU5uZ2tyN1hkYmdqSU0xUEd2NnlpQXFNMUVhR3czVUJCVEpoTHJLbGNCd05HZHR3cmtEYjd0N1V2DQpwWWFublYrUXJ5Qk1uaktMUE5keUJnWXJwL0NiNjAyZlcrQTdKa3FWTzNGTHJENnVUS0YxSlFNVzN1dVViMC9sDQo1czBpbXBmdHFwZHRuYyswZzBMRFdmMlhkNDZZN3ZkbUZ6QXRJSDZXRFFoZE01d2hmWWxRRnFpcnJxcjY3TWdODQp4RnZsdVpkRzhKbGNOTDdwS3h2QWJ2bENmNjkxdTFXeHErWlVvTk51OG5oeXNHSTFBZ01CQUFHamdZOHdnWXd3DQpDUVlEVlIwVEJBSXdBREFSQmdsZ2hrZ0JodmhDQVFFRUJBTUNCTEF3TEFZSllJWklBWWI0UWdFTkJCOFdIVTl3DQpaVzVUVTB3Z1IyVnVaWEpoZEdWa0lFTmxjblJwWm1sallYUmxNQjBHQTFVZERnUVdCQlRtNkhpYnVlODNDVkFMDQpwb3dpWk11U0ZLQW95VEFmQmdOVkhTTUVHREFXZ0JUc3ovQ0hIaXBIMkhJc1hsdWJpNWFiSEtwWVFUQU5CZ2txDQpoa2lHOXcwQkFRc0ZBQU9DQVFFQXJNT0xEcFRmd3hWWnhLVkcyK0FEQ3ZZcTcxUHF2azY4dHVJaC9vZ0dhNUc1DQo3RWxVMWxMZEwvNWJSL1ZJb1RLalR2TXRLOGRYazVsd294VWJqeUFZNDhYNGIrakxnNUNodEVPejA3U04yb1U5DQpuQVg2YklVSG9KcXB6MlZJazIrM0dVQ2htQS9HMVZqMHltWXlqT0E5Qi9qb1V3NklPVzM5bUFBeEhrU2NBNHA4DQppaGgvemIvUnRxaGRPY2UrcDd0eGx6RjFvSmRzZzFpYTNDdVl6L1AvS0oxWjB5Ujd1U3JiQ2QyVFluTzlFei9CDQo0clliRm1UanI4aUZNK0NUaFhqQ3NRUTJGa1RFNExGMzlRb291OVlMZktiOWlidnBlaGlCalJyMk40cjU3ais4DQowUTBtNGhHdkNRVk1MZDM3UE1zV2dhZkR2cjFYUEFaSUtEanM4YVFmbFE9PQ0KLS0tLS1FTkQgQ0VSVElGSUNBVEUtLS0tLQ0K"
        /// </example>>
        [Required(ErrorMessage = "certification_required")]
        public string ClientCertificationBase64 { get; set; } = "";

        /// <summary>
        /// ICCID。SIMカードに割り当てられるシリアル番号。
        /// </summary>
        /// <example>
        /// "8981100005819480000"
        /// </example>>
        [Required(ErrorMessage = "iccid_required")]
        [StringLength(19, ErrorMessage = "iccid_invalid_length")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "iccid_is_only_number")]
        public string SimIccId { get; set; } = "";

        /// <summary>
        /// IMSI。携帯電話事業者と契約する際に発行される加入者識別番号。
        /// </summary>
        /// <example>
        /// "440103213100000"
        /// </example>>
        [Required(ErrorMessage = "imsi_required")]
        [StringLength(15, ErrorMessage = "imsi_invalid_length")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "imsi_is_only_number")]
        public string SimImsi { get; set; } = "";

        /// <summary>
        /// MSISDN。携帯電話網への加入を一意に識別する番号
        /// </summary>
        /// <example>
        /// "02017911000"
        /// </example>>
        [Required(ErrorMessage = "msisdn_required")]
        [StringLength(15, ErrorMessage = "msisdn_invalid_length")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "msisdn_is_only_number")]
        public string SimMsisdn { get; set; } = "";

        /// <summary>
        /// 端末サインインで利用するアカウント。
        /// </summary>
        /// <example>
        /// "JINCREEK\\initialpoint"
        /// </example>>
        public string? Account { get; set; }
    }
}
