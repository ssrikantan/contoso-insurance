using ContosoinsExtPortal.Models;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.KeyVault.WebKey;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace ContosoinsExtPortal.Services
{
    public class AKVServiceClient
    {
        private string _clientid;
        private string _secret;
        private KeyVaultClient _keyVaultClient;
        private string _keyVaultName;
        private string _keyName;
        private string _dbConnSecretName = "dbconnstr";

    
        public AKVServiceClient(string clientid, string cerificateThumbprint, string keyVaultName, string keyName)
        {
            _clientid = clientid;
            _keyName = keyName;
            _keyVaultName = keyVaultName;
            var certificate = FindCertificateByThumbprint(cerificateThumbprint);
            var assertionCert = new ClientAssertionCertificate(clientid, certificate);
            _keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(
                 (authority, resource, scope) => GetAccessToken(authority, resource, scope, assertionCert)));
        }

      
        public async Task<bool> ValidateAsync(VehiclePolicies vehPoliciesMaster)
        {
            bool isValid = false;
            string keyUri = string.Format("https://{0}.vault.azure.net/keys/{1}", _keyVaultName, _keyName);
            string keyVaultUri = string.Format("https://{0}.vault.azure.net", _keyVaultName);
            var bundle = await _keyVaultClient.GetSecretAsync(keyVaultUri, vehPoliciesMaster.Uidname);
            string decryptedstring = bundle.Value;
            byte[] encdata = Convert.FromBase64String(decryptedstring);
            KeyOperationResult result = await _keyVaultClient.DecryptAsync(keyUri, JsonWebKeyEncryptionAlgorithm.RSAOAEP256, encdata);
            byte[] decrypteddata = result.Result;
            string secretdata = System.Text.Encoding.UTF8.GetString(decrypteddata); 
            SecretAttributes attributes = bundle.Attributes;
            if (attributes.Expires < System.DateTime.UtcNow || attributes.NotBefore > System.DateTime.UtcNow)
            {
                isValid = false;
            }
            VehiclePolicies _vehPoliciesMaster = JsonConvert.DeserializeObject<VehiclePolicies>(secretdata);

            if (vehPoliciesMaster.Id.Equals(_vehPoliciesMaster.Id) &&
              vehPoliciesMaster.Policyno.Equals(_vehPoliciesMaster.Policyno) &&
              vehPoliciesMaster.Userid.Equals(_vehPoliciesMaster.Userid) &&
              vehPoliciesMaster.Vehicleno.Equals(_vehPoliciesMaster.Vehicleno) &&
              vehPoliciesMaster.Inscompany.Equals(_vehPoliciesMaster.Inscompany)
              )
            {
                isValid = true;
            }
            else
            {
                return isValid;
            }
            return isValid;
        }

        #region internal methods

        private X509Certificate2 FindCertificateByThumbprint(string certificateThumbprint)
        {
            if (certificateThumbprint == null)
                throw new System.ArgumentNullException("certificateThumbprint");

            foreach (StoreLocation storeLocation in (StoreLocation[])
                Enum.GetValues(typeof(StoreLocation)))
            {
                foreach (StoreName storeName in (StoreName[])
                    Enum.GetValues(typeof(StoreName)))
                {
                    X509Store store = new X509Store(storeName, storeLocation);

                    store.Open(OpenFlags.ReadOnly);
                    X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindByThumbprint, certificateThumbprint, false); // Don't validate certs, since the test root isn't installed.
                    if (col != null && col.Count != 0)
                    {
                        foreach (X509Certificate2 cert in col)
                        {
                            if (cert.HasPrivateKey)
                            {
                                store.Close();
                                return cert;
                            }
                        }
                    }
                }
            }
            throw new System.Exception(
                    string.Format("Could not find the certificate with thumbprint {0} in any certificate store.", certificateThumbprint));
        }

        private async Task<string> GetAccessToken(string authority, string resource, string scope, ClientAssertionCertificate assertionCert)
        {
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var result = await context.AcquireTokenAsync(resource, assertionCert).ConfigureAwait(false);
            return result.AccessToken;
        }
        public string GetDbConnectionString()
        {
            string keyVaultUri = string.Format("https://{0}.vault.azure.net", _keyVaultName);
            SecretBundle bundle = _keyVaultClient.GetSecretAsync(keyVaultUri, _dbConnSecretName).Result;
            return bundle.Value;
        }
        #endregion

    }
}
