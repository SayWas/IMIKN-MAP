using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Essentials;

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
            Text = 0;
            ScaleValue = 0;
        }

        public Command OpenFlyoutCommand { get; }

        public Command StopCompassCommand { get; }

        public Command StartCompassCommand { get; }

        public Command ScaleUpCommand { get; }

        public Command ScaleDownCommand { get; }

        private double text;
        private double scaleValue;

        public double Text { get => text; set => SetProperty(ref text, value); }

        public double ScaleValue { get => scaleValue; set => SetProperty(ref scaleValue, value); }

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
