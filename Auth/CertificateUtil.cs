using System;
using System.Security.Cryptography.X509Certificates;
using NLog;

namespace JinCreek.Server.Auth
{
    static class CertificateUtil
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static string GetSubjectCommonNameByCertificationBase64(string base64String)
        {
            try
            {
                var generateX509Certificate2 = GenerateX509Certificate2(base64String);
                return GetSubjectCommonNameByCertification(generateX509Certificate2);
            }
            catch (Exception e)
            {
                Logger.Debug(exception: e, e.ToString());
                return null;
            }
        }

        private static X509Certificate2 GenerateX509Certificate2(string base64String)
        {
            return new X509Certificate2(Convert.FromBase64String(base64String));
        }

        private static string GetSubjectCommonNameByCertification(X509Certificate2 x509Certificate2)
        {
            return x509Certificate2.GetNameInfo(X509NameType.SimpleName, false);
        }
    }
}
