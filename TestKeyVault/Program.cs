using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Security.Cryptography.X509Certificates;

namespace TestKeyVault
{
    class Program
    {
        static void Main(string[] args)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();

            // using managed identities
            var kv = new KeyVaultClient(async (authority, resource, scope) =>
            {
                var authContext = new AuthenticationContext(authority);
                //var clientCred = new ClientCredential("67330706-0dd3-4a98-90db-4db4e23554e4", "1:NYdw1Mb19aXZrBk/mdTp[l@htk61a1");
                var clientCred = new ClientCredential("2513bdbe-5200-478f-8762-da36b874c270", "LU-M5ELVo*G0.CLEsloRZxQk2O:PMYr3");
                var result = await authContext.AcquireTokenAsync(resource, clientCred);

                if (result == null)
                    throw new InvalidOperationException("Failed to obtain the JWT token");

                return result.AccessToken;
            });
                                            

            var certificate = kv.GetCertificateAsync($"https://forwardingdevkeyvault.vault.azure.net/", "IdentityServerSigningCertificate").GetAwaiter().GetResult();
            //var certificate = kv.GetCertificateAsync($"https://reefer-dev.vault.azure.net/", "TelefonicaDev").GetAwaiter().GetResult();
        }
    }
}
