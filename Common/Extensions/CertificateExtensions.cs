using System.Security.Cryptography.X509Certificates;

namespace SpeechToolsBot.Common.Extensions;

internal static class CertificateExtensions
{
    public static X509Certificate2 GetCertificate(this string thumbPrint)
    {
        var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        try
        {
            store.Open(OpenFlags.ReadOnly);
            var certCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbPrint, false);

            if (certCollection.Count is 0)
            {
                throw new ArgumentException("Certificate Is Not Installed");
            }
            return certCollection.First();
        }
        finally
        {
            store.Close();
        }
    }
}