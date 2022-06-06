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
                    var uri = new UriBuilder("https://0x0.st/oMcc.json").Uri;
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
                    var content = new WebClient().DownloadString(uri);
                    App.Current.Properties.Add("Dots", content); }
                object rawdots;
                if (!App.Current.Properties.TryGetValue("RawDots", out rawdots)) {
                    var uri1 = new UriBuilder("https://0x0.st/oMcA.json").Uri;
                    var content1 = new WebClient().DownloadString(uri1);
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
