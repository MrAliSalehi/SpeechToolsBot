using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration.AzureKeyVault;

namespace SpeechToolsBot.Common.Extensions;

internal class SecretManager : IKeyVaultSecretManager
{
    public bool Load(SecretItem secret) => secret.Identifier.Name.StartsWith($"{StaticVariables.EnvironmentName}-");

    public string GetKey(SecretBundle secret) => secret.SecretIdentifier.Name.Replace($"{StaticVariables.EnvironmentName}-", "");
}