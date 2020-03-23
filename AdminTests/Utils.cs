using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace JinCreek.Server.AdminTests
{
    [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
    public static class Utils
    {
        private const string LoginUrl = "/api/authentication/login";

        /// <summary>
        /// アクセストークンを取得して返す
        /// </summary>
        public static string GetAccessToken(HttpClient client, string username, string password, int? organizationCode = null, string domainName = null)
        {
            var response = client.PostAsync(LoginUrl, CreateJsonContent(new { username, password, organizationCode, domainName })).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
            }
            return (string)JObject.Parse(response.Content.ReadAsStringAsync().Result)["accessToken"];
        }

        /// <summary>
        /// application/jsonのHttpContentを作る
        /// </summary>
        public static HttpContent CreateJsonContent(object obj)
        {
            return new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
        }

        /// <summary>
        /// multipart/form-dataのHttpContentを作る
        /// </summary>
        public static HttpContent CreateFormContent(string content, string name)
        {
            return new MultipartFormDataContent { { new StringContent(content), name, name } };
        }

        /// <summary>
        /// Authorization:Bearerヘッダ付きのHTTPリクエストを送信する
        /// </summary>
        private static HttpResponseMessage SendWithBearer(HttpClient client, HttpMethod method, string requestUri, HttpContent content, string bearer)
        {
            return client.SendAsync(new HttpRequestMessage(method, requestUri)
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", bearer) },
                Content = content,
            }).Result;
        }

        /// <summary>
        /// stringをJObjectにパースして返す。パースできない場合は空のJObjectを返す。
        /// </summary>
        private static JObject TryParseJObject(string json)
        {
            try
            {
                // see https://github.com/JamesNK/Newtonsoft.Json/issues/1241#issuecomment-461283844
                using var reader = new JsonTextReader(new StringReader(json)) { DateParseHandling = DateParseHandling.None };
                return JObject.Load(reader);
            }
            catch (JsonReaderException)
            {
                return new JObject();
            }
        }

        /// <summary>
        /// HttpResponseMessageからbodyと、bodyをJObjectにパースしたものを取り出して返す
        /// </summary>
        /// <returns>(response, body, json)</returns>
        private static (HttpResponseMessage, string, JObject) Extract(HttpResponseMessage response)
        {
            var body = response.Content.ReadAsStringAsync().Result;
            return (response, body, TryParseJObject(body));
        }

        public static HttpResponseMessage Get(HttpClient client, string url, string bearer) => SendWithBearer(client, HttpMethod.Get, url, null, bearer);
        public static HttpResponseMessage Delete(HttpClient client, string url, string bearer) => SendWithBearer(client, HttpMethod.Delete, url, null, bearer);
        public static HttpResponseMessage Post(HttpClient client, string url, HttpContent content, string bearer) => SendWithBearer(client, HttpMethod.Post, url, content, bearer);
        public static HttpResponseMessage Put(HttpClient client, string url, HttpContent content, string bearer) => SendWithBearer(client, HttpMethod.Put, url, content, bearer);

        public static (HttpResponseMessage, string, JObject) Get(HttpClient client, string url, string username, string password, int? organizationCode = null, string domainName = null)
            => Extract(Get(client, url, GetAccessToken(client, username, password, organizationCode, domainName)));
        public static (HttpResponseMessage, string, JObject) Delete(HttpClient client, string url, string username, string password, int? organizationCode = null, string domainName = null)
            => Extract(Delete(client, url, GetAccessToken(client, username, password, organizationCode, domainName)));
        public static (HttpResponseMessage, string, JObject) Post(HttpClient client, string url, HttpContent content, string username, string password, int? organizationCode = null, string domainName = null)
            => Extract(Post(client, url, content, GetAccessToken(client, username, password, organizationCode, domainName)));
        public static (HttpResponseMessage, string, JObject) Put(HttpClient client, string url, HttpContent content, string username, string password, int? organizationCode = null, string domainName = null)
            => Extract(Put(client, url, content, GetAccessToken(client, username, password, organizationCode, domainName)));

        public static void RemoveAllEntities(MainDbContext context)
        {
            context.RemoveRange(context.AuthenticationLog); // 認証操作
            context.RemoveRange(context.AuthenticationState); // 認証状態
            context.RemoveRange(context.MultiFactor); // 多要素組合せ
            context.RemoveRange(context.SimAndDevice); // SIM & 端末組合せ
            context.RemoveRange(context.Sim); // SIM
            context.RemoveRange(context.SimGroup); // SIMグループ

            context.RemoveRange(context.Device); // 端末
            context.RemoveRange(context.LteModule); // LTEモジュール
            context.RemoveRange(context.DeviceGroup); // 端末グループ
            context.RemoveRange(context.DeviceGroupDevice); // 端末グループ - 端末

            context.RemoveRange(context.OrganizationClientApp); // 組織端末アプリ
            context.RemoveRange(context.ClientApp); // JinCreek端末アプリ
            context.RemoveRange(context.ClientOs); // JinCreekサポート端末OS

            context.RemoveRange(context.AvailablePeriod); // 利用期間
            context.RemoveRange(context.User); // ユーザー
            context.RemoveRange(context.UserGroup); // ユーザーグループ
            context.RemoveRange(context.UserGroupEndUser); // ユーザーグループ - ユーザー
            context.RemoveRange(context.Domain); // ドメイン
            context.RemoveRange(context.Organization); // 組織
            context.SaveChanges();
        }

        public static string AddQueryString(string uri, object query)
        {
            return QueryHelpers.AddQueryString(uri, JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(query)));
        }

        /// <summary>
        /// 組織を作る
        /// </summary>
        /// <example>
        /// Utils.CreateOrganization(code: 1, name: "org1")
        /// </example>
        public static Organization CreateOrganization(int code, string name, string address = "address",
            string phone = "0123456789", string url = "https://example.com", string adminPhone = "1123456789",
            string adminMail = "admin@example.com", DateTime startDate = default, DateTime? endDate = null,
            bool isValid = true)
        {
            return new Organization
            {
                Code = code,
                Name = name,
                Address = address,
                Phone = phone,
                Url = url,
                AdminPhone = adminPhone,
                AdminMail = adminMail,
                StartDate = startDate,
                EndDate = endDate,
                IsValid = isValid,
                DistributionServerIp = "127.0.0.1"
            };
        }

        /// <summary>
        /// 端末を作る
        /// </summary>
        /// <example>
        /// Utils.CreateDevice(domain: _domain1, name: "device1", deviceGroupDevices: new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } })
        /// </example>
        public static Device CreateDevice(Domain domain, string name, Guid? id = null,
            ISet<DeviceGroupDevice> deviceGroupDevices = null, string managedNumber = "managedNumber",
            string serialNumber = "serialNumber", string productName = "productName", LteModule lteModule = null,
            OrganizationClientApp organizationClientApp = null, bool useTpm = false, int windowsSignInListCacheDays = 0)
        {
            return new Device
            {
                Id = id ?? Guid.NewGuid(),
                Domain = domain,
                Name = name,
                DeviceGroupDevices = deviceGroupDevices,
                ManagedNumber = managedNumber,
                SerialNumber = serialNumber,
                ProductName = productName,
                LteModule = lteModule,
                OrganizationClientApp = organizationClientApp,
                UseTpm = useTpm,
                WindowsSignInListCacheDays = windowsSignInListCacheDays,
            };
        }

        /// <summary>
        /// SIMグループを作る
        /// </summary>
        /// <example>
        /// Utils.CreateSimGroup(organization: _org1, name: "simGroup1")
        /// </example>
        public static SimGroup CreateSimGroup(Organization organization, string name, string isolatedNw1IpPool,
            Guid? id = null, string apn = "apn", string nasIp = "nasIp",
            string isolatedNw1IpRange = "isolatedNw1IpRange", string authenticationServerIp = "authenticationServerIp",
            string primaryDns = "primaryDns", string secondaryDns = "secondaryDns",
            string isolatedNw1PrimaryDns = "isolatedNw1PrimaryDns",
            string isolatedNw1SecondaryDns = "isolatedNw1SecondaryDns", string userNameSuffix = "userNameSuffix")
        {
            return new SimGroup
            {
                Id = id ?? Guid.NewGuid(),
                Organization = organization,
                Name = name,
                Apn = apn,
                NasIp = nasIp,
                AuthenticationServerIp = authenticationServerIp,
                IsolatedNw1IpPool = isolatedNw1IpPool,
                IsolatedNw1SecondaryDns = isolatedNw1SecondaryDns,
                IsolatedNw1IpRange = isolatedNw1IpRange,
                IsolatedNw1PrimaryDns = isolatedNw1PrimaryDns,
                PrimaryDns = primaryDns,
                SecondaryDns = secondaryDns,
                UserNameSuffix = userNameSuffix,
            };
        }

        /// <summary>
        /// SIM＆端末を作る
        /// </summary>
        /// <example>
        /// Utils.CreateSimGroup(organization: _org1, name: "simGroup1")
        /// </example>
        public static SimAndDevice CreateSimAndDevice(Sim sim, Device device, Guid? id = null,
            string isolatedNw2Ip = "IsolatedNw2Ip", int authenticationDuration = 0, DateTime startDate = default,
            DateTime? endDate = null, SimAndDeviceAuthenticated simAndDeviceAuthenticated = null)
        {
            return new SimAndDevice
            {
                Id = id ?? Guid.NewGuid(),
                Sim = sim,
                Device = device,
                IsolatedNw2Ip = isolatedNw2Ip,
                AuthenticationDuration = authenticationDuration,
                StartDate = startDate,
                EndDate = endDate,
                SimAndDeviceAuthenticated = simAndDeviceAuthenticated,
            };
        }

        public static string HashPassword(string password)
        {
            return new PasswordHasher<object>().HashPassword(null, password);
        }
    }
}
