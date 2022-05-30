using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMIKN_MAP.ViewModels;
using IMIKN_MAP.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IMIKN_MAP.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        public MapPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (!DesignMode.IsDesignModeEnabled)
                ((MapViewModel)BindingContext).StartCompassCommand.Execute(null);
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (!DesignMode.IsDesignModeEnabled)
                ((MapViewModel)BindingContext).StopCompassCommand.Execute(null);
        }
    }
}