# Using Azure Key Vault to manage and store sensitive Business information
A Solution that showcases the use of Azure Key Vault to for secure handling of Business sensitive data. To understand the context of scenario, refer to the article published in the MSDN Magazine <link to be added later>

# Steps to Deploy the Solution 

1. Create the Database instance in Azure SQL Database. Run contosoinsurance/db scripts/script.sql
2. Run the PowerShell Scripts in the folder contosoinsurance/ContosoInsAuthority/scripts. These scripts create an Azure Key Vault, create a Service Principal in Azure AD that would be have access to Key Vault, generate a Self signed certificate that would be associated with the Service Principal, and upload the certificate to the Local user's certificate store on the local machine.
  * PrepareContosoAKV.ps1 - Run this script on your Azure Subscription. (This is for the Admin portal)
     * Copy the output values for the VaultUrl, AuthClientId (Client ID of the Service Principal), AuthCertThumbprint(Thumbprint of the Certificate created for the Service Principal). 
     * These values need to be updated in the appsettings.json file in the **ContosoInsAuthorityAdminPortal.sln** Solution
  * Create a Key in the Key Vault created above, using the Azure portal. Enable the Encrypt and decrypt operations on it. Copy the name of the Key created into the appsettings.json file in the Visual Studio Solution.
  * Create a Secret in the Key Vault created above, with the name **dbconnstr**  and store the connection string of the Azure SQL Database created in step 1 above in the secret.
  
  * PrepareContosousersAKV.ps1 - Run this script on your Azure Subscription. (This is for the Customer Portal)
      * Copy the output values for the VaultUrl, AuthClientId (Client ID of the Service Principal), AuthCertThumbprint(Thumbprint of the Certificate created for the Service Principal). (Note: Keep the value of the Keyvault name same as in the PrepareContosoAKV.ps1 script so that we reuse the same Key Vault created earlier, and not end up creating another one here)
      * These values need to be updated in the appsettings.json file in the **ContosoinsExtPortal.sln** Solution
  * Copy the name of the Key created earlier into the appsettings.json file in the Visual Studio Solution.
  
3. Register the Admin Portal with Azure AD as a Web App (this is for User sign in experience to the Portal). Add the Redirect Url as shown in the screenshot below, for localhost url alone. 
  * Copy the ApplicationID created for the App and paster them into the appsettings.json file in the **ContosoInsAuthorityAdminPortal.sln** Solution.

![GitHub Logo](/images/WebAppRegistration1.png)

  * Update the appsettings.json file in the **ContosoInsAuthorityAdminPortal.sln** Solution.
   ````json
   "AzureAd": {
       "Instance": "https://login.microsoftonline.com/",
       "Domain": "Your Azure AD Tenant Domain",
       "TenantId": "Your Azure AD Tenant ID",
       "ClientId": "Application ID for the Web Application registered above",
       "CallbackPath": "/signin-oidc"
     }
   ````
 4. Create an Azure AD B2C Tenant. Follow the steps mentioned in the documentation link here https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-get-started
 5. Register the Customer Portal with this Azure AD B2C Tenant, as a Web App (This is for the Customer users Signup and sign in experience). Follows the steps mentioned in the Documentation link here https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-app-registration. The redirect URL configuration for the Localhost URL should be changed to the values shown in the screenshot below.
  * Define the 'Signin or Up' policy & the 'Reset Password' Policy for this Web Application. Refer to this link for guidance in performing these steps. - https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-reference-policies. In this case, the names of these policies created are B2C_1_siorsup and B2C_1_reset respectively (see JSON fragment below)
![GitHub Logo](/images/WebAppRegistration2.png)

  ````json
   "AzureAdB2C": {
    "Instance": "https://login.microsoftonline.com/tfp",
    "ClientId": "Application ID for the Web App registered in Azure AD B2C above",
    "CallbackPath": "/signin-oidc",
    "Domain": "<Your B2C Tenant>.onmicrosoft.com",
    "SignUpSignInPolicyId": "B2C_1_siorsup",
    "ResetPasswordPolicyId": "B2C_1_reset",
    "EditProfilePolicyId": ""
  }
   `````
   * Set the Identity Provider for the Azure AD B2C Tenant to 'User Name'. See the screen shot below    
    ![GitHub Logo](/images/IdentityProviders.png)
   * Sign up some Users to the Azure AD B2C Tenant that can be used to test the application
  6. Run both the Admin portal and Customer Portal Applications from Visual Studio 2017 on the local machine and ensure that it works
  7. Publish the Applications to Azure App Service Web Apps, from Visual Studio 2017. Add the redirect URLs in the Web Apps configuration registered in Azure AD and Azure AD B2C for the Admin Portal and Customer Portal respectively
    * Upload the certificates (.pfx files) generated during the execution of the PowerShell Scripts above to the App Service instance
    * Add the thumbprint values for the above certificates into the App Service Settings. Refer to https://docs.microsoft.com/en-us/azure/key-vault/key-vault-use-from-web-application for guidance in performing these steps

# Steps to run the Solution 
1. Log in to the Admin portal https://contosoinsadminportal.azurewebsites.net using the credentials below
user name - insadmin@contosoinsusers.onmicrosoft.com
password - inscontoso@123

2. Create an Insurance policy and associate that to a Consumer and capture Policy details in the process. 
Launch 'Create Policy' page. Since there are no look ups implemented in the page yet, you would need to enter the Username of the Customer manually (plaker) in the screenshot below.
![GitHub Logo](/images/CreatePolicy.png)

3. Log in to the Consumer portal 
Sign in as Paul laker to whom the Insurance Policy created in the previous step was allocated to
user name plaker@insurancecotenant.onmicrosoft.com
password inscontoso@123
Policies that have been associated to his account are ready for activation
4.
