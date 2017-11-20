using ContosoInsAuthorityAdminPortal.Models;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.KeyVault.WebKey;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace ContosoInsAuthorityAdminPortal.Services
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
        
        //Get the Connection string to the Azure SQL Database
        public string GetDbConnectionString()
        {
            string keyVaultUri = string.Format("https://{0}.vault.azure.net", _keyVaultName);
            SecretBundle bundle = _keyVaultClient.GetSecretAsync(keyVaultUri, _dbConnSecretName).Result;
            return bundle.Value;
        }

        public async Task CreateSecret(VehiclePolicies policydata)
        {
            // Create the content for the Policy data to be stored as Secret
            Insdata akvdata = new Insdata {Id = policydata.Id, Inscompany=policydata.Inscompany,
                Policyno =policydata.Policyno, Userid=policydata.Userid, Vehicleno=policydata.Vehicleno };

            //Create a JSON String of the Policy data to be stored as Secret
            string insurancepolicysecret = JsonConvert.SerializeObject(akvdata);

            byte[] datatoencrypt = System.Text.Encoding.UTF8.GetBytes(insurancepolicysecret);
            string keyUri = string.Format("https://{0}.vault.azure.net/keys/{1}",_keyVaultName,_keyName);
            string keyVaultUri = string.Format("https://{0}.vault.azure.net", _keyVaultName);


            //Encrypt the data before it is stored as a Secret
            KeyOperationResult result = await _keyVaultClient.EncryptAsync(keyUri, JsonWebKeyEncryptionAlgorithm.RSAOAEP256, 
                    datatoencrypt);
            byte[] encdata = result.Result;
            string encrypteddata = Convert.ToBase64String(encdata);

            //Set the Policy Start and Expiry Data to be added as attributes to the secret
            SecretAttributes attribs = new SecretAttributes
            {
                Enabled = true,
                Expires = DateTime.UtcNow.AddYears(1),
                NotBefore = DateTime.UtcNow
            };

            IDictionary<string, string> alltags = new Dictionary<string, string>
            {
                { "InsuranceCompany", policydata.Inscompany }
            };
            string contentType = "DigitalInsurance";
            
            // Create a Secret with the encrypted Policy data
            SecretBundle bundle= await _keyVaultClient.SetSecretAsync(keyVaultUri, policydata.Uidname, 
                encrypteddata,alltags,contentType,attribs);
            string bundlestr = bundle.Value;

            policydata.Version = bundle.SecretIdentifier.Version;
            policydata.Lastmod = bundle.Attributes.Updated;
            policydata.Startdate = bundle.Attributes.NotBefore;
            policydata.Enddate = bundle.Attributes.Expires;
        }

        public async Task UpdateSecret(VehiclePolicies policydata)
        {
            //Create the updated Policy data to be stored as a new version of the Secret
            Insdata akvdata = new Insdata
            {
                Id = policydata.Id,
                Inscompany = policydata.Inscompany,
                Policyno = policydata.Policyno,
                Userid = policydata.Userid,
                Vehicleno = policydata.Vehicleno
            };

            //Create the JSON String of the updated Policy Object
            string insurancepolicysecret = JsonConvert.SerializeObject(akvdata);
            byte[] datatoencrypt = System.Text.Encoding.UTF8.GetBytes(insurancepolicysecret);
            string keyUri = string.Format("https://{0}.vault.azure.net/keys/{1}", _keyVaultName, _keyName);
            string keyVaultUri = string.Format("https://{0}.vault.azure.net", _keyVaultName);


            KeyOperationResult result = null;
            //Get the metadata from the existing Secret in Key Vault
            SecretBundle bundle = await _keyVaultClient.GetSecretAsync(keyVaultUri, policydata.Uidname);
            if(bundle==null)
            {
                throw new ApplicationException("Error locating Secret data to update");
                //No need to execute the rest of the steps if the Secret cannot be retrieved
            }
            SecretAttributes _attribs = bundle.Attributes;
            string _contentType = bundle.ContentType;
            IDictionary<string, string> dic = bundle.Tags;

            //Create the attributes for the updated Secret
            SecretAttributes attribsNew = new SecretAttributes
            {
                Enabled = true,
                Expires = _attribs.Expires,
                NotBefore = DateTime.UtcNow
            };

            IDictionary<string, string> alltags = dic;
            string contentType = _contentType;
          
            // Encrypt the updated Secret data
            result = await _keyVaultClient.EncryptAsync(keyUri, JsonWebKeyEncryptionAlgorithm.RSAOAEP256,
                    datatoencrypt);
            byte[] encdata = result.Result;
            string encrypteddata = Convert.ToBase64String(encdata);
            
            //Create a new version of the Secret by calling the SetSecret Method, and using the attributes from the previous version of the Secret
            bundle = await _keyVaultClient.SetSecretAsync(keyVaultUri, policydata.Uidname,
                encrypteddata,alltags,contentType,attribsNew);
            string bundlestr = bundle.Value;

            policydata.Version = bundle.SecretIdentifier.Version;
            policydata.Lastmod = bundle.Attributes.Updated;
            policydata.Startdate = bundle.Attributes.NotBefore;
            policydata.Enddate = bundle.Attributes.Expires;
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
        #endregion

        #region unusedcode
        /// <summary>
        /// This contructor is not used in the Solution
        /// </summary>
        /// <param name="clientid"></param>
        /// <param name="secret"></param>
        /// <param name="keyVaultName"></param>
        /// <param name="keyName"></param>
        /// <param name="flag"></param>
        public AKVServiceClient(string clientid, string secret, string keyVaultName, string keyName, bool flag)
        {
            _clientid = clientid;
            _secret = secret;
            _keyName = keyName;
            _keyVaultName = keyVaultName;
            _keyVaultClient = new KeyVaultClient(GetAccessToken);
        }

        private async Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var clientCredential = new ClientCredential(_clientid, _secret);
            var result = await authContext.AcquireTokenAsync(resource, clientCredential);
            var token = result.AccessToken;
            return token;
        }

        #endregion
    }
}
