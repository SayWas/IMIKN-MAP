using IMIKN_MAP.Models;
using IMIKN_MAP.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Essentials;
using IMIKN_MAP.Services;

namespace IMIKN_MAP.ViewModels
{
    class MapViewModel : BaseViewModel
    {
        public MapViewModel()
        {
            StopCompassCommand = new Command(StopCompass);
            StartCompassCommand = new Command(StartCompass);
            OpenFlyoutCommand = new Command(OpenFlyout);
            ScaleUpCommand = new Command(ScaleUp);
            ScaleDownCommand = new Command(ScaleDown);
            ChoosePathCommand = new Command(ChoosePath);
            Text = 0;
            ScaleValue = 0;
            Target = new double[3];
        }

        public Command OpenFlyoutCommand { get; }
        public Command ChoosePathCommand { get; }
        public Command StopCompassCommand { get; }
        public Command StartCompassCommand { get; }
        public Command ScaleUpCommand { get; }
        public Command ScaleDownCommand { get; }

        private Dot[] navigationDots;
        private double[] target;
        private double text;
        private double scaleValue;

        public double Text { get => text; set => SetProperty(ref text, value); }
        public double ScaleValue { get => scaleValue; set => SetProperty(ref scaleValue, value); }
        public Dot[] NavigationDots { get => navigationDots; set => SetProperty(ref navigationDots, value); }
        public double[] Target { get => target; set => SetProperty(ref target, value); }

        private void StopCompass()
        {
            if (!Compass.IsMonitoring)
                return;
            Compass.ReadingChanged -= Compass_ReadingChanged;
            Compass.Stop();
        }
        private void StartCompass()
        {
            if (Compass.IsMonitoring)
                return;
            Compass.ReadingChanged += Compass_ReadingChanged;
            Compass.Start(SensorSpeed.Game);
        }
        private void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            Text = e.Reading.HeadingMagneticNorth;
        }

        private void OpenFlyout()
        {
            Shell.Current.FlyoutIsPresented = true;
        }
        private async void ChoosePath()
        {
            await Shell.Current.GoToAsync(nameof(PathSelectionPage));
        }

        private void ScaleUp()
        {
            ScaleValue = 1.5;
            ScaleValue = 0;
        }
        private void ScaleDown()
        {
            ScaleValue = 0.75;
            ScaleValue = 0;
        }
    }
}
