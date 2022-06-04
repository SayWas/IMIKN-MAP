using IMIKN_MAP.Services;
using IMIKN_MAP.Views;
using System;
using System.Net;
using System.Net.Security;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IMIKN_MAP
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            try
            {
                object dots;
                if (!App.Current.Properties.TryGetValue("Dots", out dots))
                {
                    var uri = new UriBuilder("https://0x0.st/oMHx.json").Uri;
                    var client = new WebClient();
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
                    var content = client.DownloadString(uri);
                    App.Current.Properties.Add("Dots", content);
                    var uri1 = new UriBuilder("https://0x0.st/oMH6.json").Uri;
                    var client1 = new WebClient();
                    var content1 = client.DownloadString(uri1);
                    App.Current.Properties.Add("RawDots", content1);
                }
            }
            catch (WebException wbex)
            {
                Console.WriteLine(wbex.Message);
            }
            catch (Exception)
            {

            }
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
