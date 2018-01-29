using System;
using System.Collections.Generic;
using Microsoft.Identity.Client;

using Xamarin.Forms;

namespace Scanner
{
    public partial class WelcomePage : ContentPage
    {
        public WelcomePage()
        {
            InitializeComponent();
        }
        protected override void OnAppearing()
        {
            //base.OnAppearing();
            try
            {
                PublicClientApplication publicClientApplication = new PublicClientApplication(AuthParameters.Authority, AuthParameters.ClientId);
                var authResult =  publicClientApplication.AcquireTokenSilentAsync(AuthParameters.Scopes, "", AuthParameters.Authority, AuthParameters.Policy, false);
                 Navigation.PushAsync(new Scanner());
            }
            catch
            {

            }
        }
    }
}
