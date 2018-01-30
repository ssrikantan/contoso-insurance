using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;



namespace Scanner
{   
    public class QRCodeData {
        
        public string Inscompany { get; set; }
        public string Policyno { get; set; }
        public string Vehicleno { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Enddate { get; set; }
        public string Startdate { get; set; }
        public string Id { get; set; }
        //public string qrcode { get; set; }
    }
    public partial class QRResultPage : ContentPage
    {
        public static string jsondata;

        /*void Handle_Clicked(object sender, System.EventArgs e)
        {

            //Post data to Web Service and get the validation result
            async DoValidate();
        }*/

        public async Task DoValidate()
        {
            string accesstoken = MainPage.authResult.IdToken;   
           
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://contosoinsusers.azurewebsites.net");
            string jsonData = jsondata;
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            //content.Headers.Add("authorization", "Bearer "+accesstoken);
            //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("", "");
            client.DefaultRequestHeaders.Add("authorization", "Bearer " + accesstoken);
            HttpResponseMessage response = await client.PostAsync("/api/validation", content);

            // this result string should be something like: "{"token":"rgh2ghgdsfds"}"
            var result = await response.Content.ReadAsStringAsync();
            if(result == "true"){
                await DisplayAlert("Validation Result", "Passed", "OK");
            }
            else {
                 await DisplayAlert("Validation Result", "Failed", "OK");
            }

        }

        public QRResultPage(QRResult qrresult)
        {
            InitializeComponent();
            var qrdata = JsonConvert.DeserializeObject<QRCodeData>(qrresult.qrcode);
            lblFNameValue.Text = qrdata.Firstname;
            lblLNameValue.Text = qrdata.Lastname;
            lblPolicyNoValue.Text = qrdata.Policyno;
            lblVehicleNoValue.Text = qrdata.Vehicleno;
            lblEndDateValue.Text = qrdata.Enddate;
            lblInsCompanyValue.Text = qrdata.Inscompany;
            jsondata = qrresult.qrcode;
            btnValidate.Clicked += async(object sender, EventArgs e) =>
            {
                await DoValidate();
            };
           
        }

        private IUser GetUserByPolicy(IEnumerable<IUser> users, string policy)
        {
            foreach (var user in users)
            {
                string userIdentifier = Base64UrlDecode(user.Identifier.Split('.')[0]);
                if (userIdentifier.EndsWith(policy.ToLower())) return user;
            }

            return null;
        }

        private string Base64UrlDecode(string s)
        {
            s = s.Replace('-', '+').Replace('_', '/');
            s = s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');
            var byteArray = Convert.FromBase64String(s);
            var decoded = Encoding.UTF8.GetString(byteArray, 0, byteArray.Count());
            return decoded;
        }

       
    }
}
