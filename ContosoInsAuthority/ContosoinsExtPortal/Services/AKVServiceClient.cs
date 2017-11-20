using ContosoinsExtPortal.Models;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.KeyVault.WebKey;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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

        //public AKVServiceClient(string clientid, string secret, string keyVaultName, string keyName)
        //{
        //    _clientid = clientid;
        //    _secret = secret;
        //    _keyName = keyName;
        //    _keyVaultName = keyVaultName;
        //    _keyVaultClient = new KeyVaultClient(GetAccessToken);
        //}

        public AKVServiceClient(string clientid, string cerificateThumbprint, string keyVaultName, string keyName)
        {
            _clientid = clientid;
            _keyName = keyName;
            _keyVaultName = keyVaultName;
            var certificate = FindCertificateByThumbprint(cerificateThumbprint);
            var assertionCert = new ClientAssertionCertificate(clientid, certificate);
            _keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(
                 (authority, resource, scope) => GetAccessToken(authority, resource, scope, assertionCert)));
            //_keyVaultClient = new KeyVaultClient(GetAccessToken);
        }


        private async Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            // var ClientSecretWeb = "p4uo2ECfidowgGaXGNOjPaHJ2cq9R9oc74pqTXof4nc=";
            // var ClientIdWeb = "652f7666-a054-4c6b-bcf7-13277a1492bb";
            var authContext = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var clientCredential = new ClientCredential(_clientid, _secret);
            var result = await authContext.AcquireTokenAsync(resource, clientCredential);
            var token = result.AccessToken;
            return token;
        }

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

        public async Task CreateSecret(VehiclePolicies policydata)
        {
            InsuranceData akvdata = new InsuranceData
            {
                Id = policydata.Id,
                Inscompany = policydata.Inscompany,
                Policyno = policydata.Policyno,
                //Userid = policydata.Userid,
                Vehicleno = policydata.Vehicleno
            };
            string insurancepolicysecret = JsonConvert.SerializeObject(akvdata);
            //insurancepolicysecret = "{\"name\":\"tester\",\"Uidname\":\"opp0901\",\"Version\":\"vfs909\"}";
            string encrypteddata = string.Empty;
            byte[] datatoencrypt = System.Text.Encoding.UTF8.GetBytes(insurancepolicysecret);
            //https://contosoinsauthkv.vault.azure.net/keys/contosodefkey/e24d2466714d466785f63dc05e4d196c
            string keyUri = string.Format("https://{0}.vault.azure.net/keys/{1}", _keyVaultName, _keyName);
            string keyVaultUri = string.Format("https://{0}.vault.azure.net", _keyVaultName);

            // string keyUri = "https://"+_keyVaultName+".vault.azure.net/keys/"+ _keyName;
            // string keyVaultUri = "https://"+_keyVaultName+".vault.azure.net";
            KeyOperationResult result = null;
            byte[] encdata = null;
            try
            {
                result = await _keyVaultClient.EncryptAsync(keyUri, JsonWebKeyEncryptionAlgorithm.RSAOAEP256,
                    datatoencrypt);
                encdata = result.Result;
                encrypteddata = Convert.ToBase64String(encdata);
            }
            catch (Exception ex)
            {
                string exc = ex.StackTrace;
            }

            SecretAttributes attribs = new SecretAttributes
            {
                Enabled = true,
                Expires = DateTime.UtcNow.AddYears(1),
                NotBefore = DateTime.UtcNow.AddYears(1)
            };

            IDictionary<string, string> alltags = new Dictionary<string, string>();
            alltags.Add("InsuranceCompany", policydata.Inscompany);
            string contentType = "DigitalInsurance";
            SecretBundle bundle = await _keyVaultClient.SetSecretAsync(keyVaultUri, policydata.Uidname,
                encrypteddata, alltags, contentType, attribs);
            string bundlestr = bundle.Value;

            policydata.Version = bundle.SecretIdentifier.Version;
            policydata.Lastmod = bundle.Attributes.Updated;

            //bundle = await _keyVaultClient.GetSecretAsync(keyVaultUri, policydata.Uidname);
            //string decryptedstring = bundle.Value;
            //encdata = Convert.FromBase64String(decryptedstring);
            //result = await _keyVaultClient.DecryptAsync(keyUri, JsonWebKeyEncryptionAlgorithm.RSAOAEP256, encdata);
            //byte[] decrypteddata = result.Result;
            //string secretdata = System.Text.Encoding.UTF8.GetString(decrypteddata);
            //return secretdata;
        }

        public async Task UpdateSecret(VehiclePolicies policydata)
        {
            InsuranceData akvdata = new InsuranceData
            {
                Id = policydata.Id,
                Inscompany = policydata.Inscompany,
                Policyno = policydata.Policyno,
                //Userid = policydata.Userid,
                Vehicleno = policydata.Vehicleno
            };
            string insurancepolicysecret = JsonConvert.SerializeObject(akvdata);
            //insurancepolicysecret = "{\"name\":\"tester\",\"Uidname\":\"opp0901\",\"Version\":\"vfs909\"}";
            string encrypteddata = string.Empty;
            byte[] datatoencrypt = System.Text.Encoding.UTF8.GetBytes(insurancepolicysecret);
            //https://contosoinsauthkv.vault.azure.net/keys/contosodefkey/e24d2466714d466785f63dc05e4d196c
            string keyUri = string.Format("https://{0}.vault.azure.net/keys/{1}", _keyVaultName, _keyName);
            string keyVaultUri = string.Format("https://{0}.vault.azure.net", _keyVaultName);


            KeyOperationResult result = null;

            //Get the metadata from the existing Secret in Key Vault
            SecretBundle bundle = await _keyVaultClient.GetSecretAsync(keyVaultUri, policydata.Uidname);
            SecretAttributes _attribs = bundle.Attributes;
            string _contentType = bundle.ContentType;
            IDictionary<string, string> dic = bundle.Tags;
            //string decryptedstring = bundle.Value;

            SecretAttributes attribsNew = new SecretAttributes
            {
                Enabled = true,
                Expires = _attribs.Expires,
                NotBefore = DateTime.UtcNow.AddDays(1)
            };

            IDictionary<string, string> alltags = dic;
            string contentType = _contentType;
            byte[] encdata = null;
            try
            {
                result = await _keyVaultClient.EncryptAsync(keyUri, JsonWebKeyEncryptionAlgorithm.RSAOAEP256,
                    datatoencrypt);
                encdata = result.Result;
                encrypteddata = Convert.ToBase64String(encdata);
            }
            catch (Exception ex)
            {
                string exc = ex.StackTrace;
            }

            bundle = await _keyVaultClient.SetSecretAsync(keyVaultUri, policydata.Uidname,
                encrypteddata, alltags, contentType, attribsNew);
            string bundlestr = bundle.Value;

            policydata.Version = bundle.SecretIdentifier.Version;
            policydata.Lastmod = bundle.Attributes.Updated;

            //bundle = await _keyVaultClient.GetSecretAsync(keyVaultUri, policydata.Uidname);
            //string decryptedstring = bundle.Value;
            //encdata = Convert.FromBase64String(decryptedstring);
            //result = await _keyVaultClient.DecryptAsync(keyUri, JsonWebKeyEncryptionAlgorithm.RSAOAEP256, encdata);
            //byte[] decrypteddata = result.Result;
            //string secretdata = System.Text.Encoding.UTF8.GetString(decrypteddata);
            //return secretdata;
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

    }
}
