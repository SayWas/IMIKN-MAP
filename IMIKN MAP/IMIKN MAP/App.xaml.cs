using IMIKN_MAP.Services;
using IMIKN_MAP.Views;
using System;
using System.Net;
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
            /*try
            {
                string serverDataVersion = new WebClient().DownloadString("где хранится dataversion");
                object dataVersion;
                if (App.Current.Properties.TryGetValue("data_version", out dataVersion) && (string)dataVersion != serverDataVersion)
                {

                    // Действия по обновлению данных

                    dataVersion = serverDataVersion;
                }
            }
            catch (WebException wbex)
            {
                Console.WriteLine(wbex.Message);
            }
            catch (Exception)
            {

            }*/
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
