using IMIKN_MAP.Models;
using IMIKN_MAP.Controls;
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
        public static MapViewModel Current;

        public MapViewModel()
        {
            StopCompassCommand = new Command(StopCompass);
            StartCompassCommand = new Command(StartCompass);
            OpenFlyoutCommand = new Command(OpenFlyout);
            ScaleUpCommand = new Command(ScaleUp);
            ScaleDownCommand = new Command(ScaleDown);
            ChoosePathCommand = new Command(ChoosePath);
            ChangeFloorCommand = new Command<SvgImage>(ChangeFloor);
            Text = 0;
            ScaleValue = 0;
            CurrentFloor = 1;
            floorIcon = new string[4] { "1_floor_glow", "2_floor", "3_floor", "4_floor" };
            Current = this;
        }

        public Command OpenFlyoutCommand { get; }
        public Command ChoosePathCommand { get; }
        public Command StopCompassCommand { get; }
        public Command StartCompassCommand { get; }
        public Command ScaleUpCommand { get; }
        public Command ScaleDownCommand { get; }
        public Command<SvgImage> ChangeFloorCommand { get; }

        private string[] pathIds;
        private double text;
        private double scaleValue;
        private string[] floorIcon;
        private int currentFloor;

        public double Text { get => text; set => SetProperty(ref text, value); }
        public double ScaleValue { get => scaleValue; set => SetProperty(ref scaleValue, value); }
        public string[] PathIds { get => pathIds; set => SetProperty(ref pathIds, value); }
        public string FloorIcon1 { get => floorIcon[0]; set => SetProperty(ref floorIcon[0], value); }
        public string FloorIcon2 { get => floorIcon[1]; set => SetProperty(ref floorIcon[1], value); }
        public string FloorIcon3 { get => floorIcon[2]; set => SetProperty(ref floorIcon[2], value); }
        public string FloorIcon4 { get => floorIcon[3]; set => SetProperty(ref floorIcon[3], value); }
        public int CurrentFloor { get => currentFloor; set => SetProperty(ref currentFloor, value); }

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
        private void ChangeFloor(SvgImage image)
        {
            if (FloorIcon1.Contains("glow")) FloorIcon1 = "1_floor";
            if (FloorIcon2.Contains("glow")) FloorIcon2 = "2_floor";
            if (FloorIcon3.Contains("glow")) FloorIcon3 = "3_floor";
            if (FloorIcon4.Contains("glow")) FloorIcon4 = "4_floor";
            if (!image.Source.Contains("glow")) image.Source += "_glow";
            if (FloorIcon1.Contains("glow") && FloorIcon1[0] == '1') CurrentFloor = 1;
            if (FloorIcon2.Contains("glow") && FloorIcon2[0] == '2') CurrentFloor = 2;
            if (FloorIcon3.Contains("glow") && FloorIcon3[0] == '3') CurrentFloor = 3;
            if (FloorIcon4.Contains("glow") && FloorIcon4[0] == '4') CurrentFloor = 4;
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
