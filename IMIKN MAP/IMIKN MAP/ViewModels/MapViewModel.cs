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
            Text = 0;
        }

        public Command StopCompassCommand { get; }

        public Command StartCompassCommand { get; }

        private double text;

        public double Text { get => text; set => SetProperty(ref text, value); }

        void StopCompass()
        {
            if (!Compass.IsMonitoring)
                return;
            Compass.ReadingChanged -= Compass_ReadingChanged;
            Compass.Stop();
        }

        void StartCompass()
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
    }
}
