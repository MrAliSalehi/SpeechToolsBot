﻿using System.Security.Cryptography.X509Certificates;

namespace SpeechToolsBot.Common.Extensions;

internal static class CertificateExtensions
{
    public static X509Certificate2 GetCertificate(this string thumbPrint)
    {
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

        store.Open(OpenFlags.ReadOnly);
        var certCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbPrint, false);

        if (certCollection.Count is 0)
        {
            throw new ArgumentException($"Certificate Is Not Installed-Store location:[{store.Location}]\nall Cert Count:{store.Certificates.Count}");
        }
        return certCollection.First();
    }
}