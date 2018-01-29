using System;
using System.Collections.Generic;

using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;
using Newtonsoft.Json;

namespace Scanner {
    public partial class PopUpPage : ContentPage {
        public PopUpPage() {
            InitializeComponent();
        }

        public void OnClickScan(object sender, EventArgs e)
        {
            // ScanAsync();
            //Bypass scan for testing purpose
            QRCodeData qrdata = new QRCodeData();
            qrdata.Inscompany = "Unitedinsc";
            qrdata.Policyno = "POL893122";
            qrdata.Vehicleno = "MH09A6677";
            qrdata.Firstname = "Paul";
            qrdata.Lastname = "Laker";
            qrdata.Startdate = "2017-11-20T14:33:05";
            qrdata.Enddate = "2018-11-20T01:41:15";
            qrdata.Id = "f87ddd46-3168-4c9f-809c-eb6261e054f6";

            var qrresultjson = JsonConvert.SerializeObject(qrdata);
            var qrresult = new QRResult
            {
                qrcode = qrresultjson
             }; 
            Navigation.PushAsync(new QRResultPage(qrresult));
        }

        public async void ScanAsync() {

            var scanPage = new ZXingScannerPage();

            scanPage.OnScanResult += (result) =>
            {
                // Stop scanning
                scanPage.IsScanning = false;
                var qrresult = new QRResult
                {
                   /* Name = "Bindu",
                    Age = "16",
                    Location = "Not in earch",*/
                    qrcode = result.Text
            };
                // Pop the page and show the result
                Device.BeginInvokeOnMainThread(() => {
                   Navigation.PopAsync();
                    DisplayAlert("Scanned Barcode", result.Text, "OK");
                    Navigation.PushAsync(new QRResultPage(qrresult));

                });
            };

            // Navigate to our scanner page
            await Navigation.PushAsync(scanPage);

        }
    }
}
