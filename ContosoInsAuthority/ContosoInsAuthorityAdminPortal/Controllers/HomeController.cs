using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContosoInsAuthorityAdminPortal.Models;


using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.KeyVault.WebKey;
using Microsoft.IdentityModel.Clients.ActiveDirectory;


using System.Security.Claims;

namespace ContosoInsAuthorityAdminPortal.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
  //      private KeyVaultClient keyVaultClient;
  //      private string kvName = "contosoInsAuthKV";
  //      private string keyName = "contosodefkey";

  //      private string Keyidentifier = "https://contosoinsauthkv.vault.azure.net/keys/contosodefkey/e24d2466714d466785f63dc05e4d196c";
        public IActionResult Index()
        {
            //keyVaultClient = new KeyVaultClient(GetAccessToken);

            //string text = "This data needs to be encrypted";
            //byte[] edata = System.Text.Encoding.UTF8.GetBytes(text);


            //KeyOperationResult operationResult;
            //var algorithm = JsonWebKeyEncryptionAlgorithm.RSAOAEP256;
            //try
            //{
            //    operationResult = Task.Run(() => keyVaultClient.EncryptAsync(Keyidentifier, algorithm, edata)).ConfigureAwait(false).GetAwaiter().GetResult();
            //    Console.Out.WriteLine(string.Format("The encrypted text is: {0}", System.Text.Encoding.UTF8.GetString(operationResult.Result)));

            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Exception during decryption " + ex.StackTrace);
            //}

            return View();
        }

        private async Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            //TokenCache.DefaultShared.Clear();
            
            //string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

            var authContext = new AuthenticationContext(authority, TokenCache.DefaultShared);
            //var clientCredential = new ClientCredential(ClientIdWeb, ClientSecretWeb);
            string username = User.Identity.Name;
          
            var result = await authContext.AcquireTokenAsync(resource, "c2ae2108-6f55-4f20-920f-906513fd1223", new UserCredential());
            var token = result.AccessToken;
            return token;
        }

        public IActionResult About()
        {

            ViewData["Message"] = "Your application description page.";
          
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
