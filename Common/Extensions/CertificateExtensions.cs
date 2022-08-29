using System.Security.Cryptography.X509Certificates;

namespace SpeechToolsBot.Common.Extensions;

internal static class CertificateExtensions
{
    public static X509Certificate2 GetCertificate(this string thumbPrint)
    {
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

        store.Open(OpenFlags.ReadOnly);

        var certCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbPrint, false);

        if (certCollection.Count is not 0)
            return certCollection.First();

        Log.Error("name:[{FriendlyName}]subjName:[{SubjectNameName}]",
            store.Certificates.First().FriendlyName, store.Certificates.First().SubjectName.Name);
        throw new ArgumentException($"Certificate Is Not Installed\nall Cert Count:{store.Certificates.Count}");
    }
}