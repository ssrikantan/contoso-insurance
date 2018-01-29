using System;
using Xamarin.Forms;
using Microsoft.Identity.Client;

namespace Scanner {
    public partial class App : Application {

        /* Added for Azure AD B2C Auth */
          public static PublicClientApplication PCA = null;
        
        // Azure AD B2C Coordinates
        /*public static string Tenant = "insudemo1.onmicrosoft.com";
        public static string ClientID = "b002ae70-3252-472f-ad16-a09edd1f952c";
        public static string PolicySignUpSignIn = "B2C_1_email_singup_or_signin";
        public static string PolicyEditProfile = "B2C_1_email_edit_profile";
        public static string PolicyResetPassword = "B2C_1_password_reset";

        public static string[] Scopes = { "https://fabrikamb2c.onmicrosoft.com/demoapi/demo.read" };
        public static string ApiEndpoint = "https://fabrikamb2chello.azurewebsites.net/hello";

        public static string AuthorityBase = $"https://login.microsoftonline.com/tfp/{Tenant}/";
        public static string Authority = $"{AuthorityBase}{PolicySignUpSignIn}";
        public static string AuthorityEditProfile = $"{AuthorityBase}{PolicyEditProfile}";
        public static string AuthorityPasswordReset = $"{AuthorityBase}{PolicyResetPassword}";

        public static UIParent UiParent = null;*/
        public static string Tenant = "insurancecotenant.onmicrosoft.com";
        public static string ClientID = "6106006e-cbbb-43bf-acbb-af1800bcfd10";
        public static string PolicySignUpSignIn = "B2C_1_siorsup";
        //public static string PolicyEditProfile = "B2C_1_email_edit_profile";
        public static string PolicyResetPassword = "B2C_1_reset";

        public static string[] Scopes = { "https://fabrikamb2c.onmicrosoft.com/demoapi/demo.read" };
        public static string ApiEndpoint = "https://fabrikamb2chello.azurewebsites.net/hello";

        public static string AuthorityBase = $"https://login.microsoftonline.com/tfp/{Tenant}/";
        public static string Authority = $"{AuthorityBase}{PolicySignUpSignIn}";
        //public static string AuthorityEditProfile = $"{AuthorityBase}{PolicyEditProfile}";
        public static string AuthorityPasswordReset = $"{AuthorityBase}{PolicyResetPassword}";

        public static UIParent UiParent = null;


        public App() {
           
            //var startPage = new Scanner();
            //Changed - to include WelcomePage with Social Login
            //MainPage = new NavigationPage(startPage);
            //MainPage = new NavigationPage(new WelcomePage());

            // default redirectURI; each platform specific project will have to override it with its own
            //var startPage = new PopUpPage();
            PCA = new PublicClientApplication(ClientID, Authority);
            PCA.RedirectUri = $"msal{ClientID}://auth";
            MainPage = new NavigationPage(new MainPage());  

        }
    }
    //Not Used
    public class AuthParameters
    {
        public const string Authority = "https://login.microsoftonline.com/insudemo1.onmicrosoft.com/";
        public const string ClientId = "b002ae70-3252-472f-ad16-a09edd1f952c";
        public static readonly string[] Scopes = { ClientId };
        public const string Policy = "B2C_1_email_singup_or_signin";
    }
}
