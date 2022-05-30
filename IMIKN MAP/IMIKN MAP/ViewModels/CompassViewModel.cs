using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace IMIKN_MAP.ViewModels
{
    class CompassViewModel : BaseViewModel
    {
        public CompassViewModel()
        {
            StopCommand = new Command(Stop);
            StartCommand = new Command(Start);
            Text = 0;
        }

        public Command StopCommand { get; }

        public Command StartCommand { get; }

        private double text;

        public double Text { get => text; set => SetProperty(ref text, value); }

        void Stop()
        {
            if (!Compass.IsMonitoring)
                return;
            Compass.ReadingChanged -= Compass_ReadingChanged;
            Compass.Stop();
        }

        void Start()
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
