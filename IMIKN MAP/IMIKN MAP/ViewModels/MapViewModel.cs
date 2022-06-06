using IMIKN_MAP.Models;
using IMIKN_MAP.Controls;
using IMIKN_MAP.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Essentials;
using IMIKN_MAP.Services;
using System.Threading;
using System.Threading.Tasks;

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
            RotationAngle = 0;
            ScaleValue = 0;
            floorIcon = new string[4] { "1_floor_glow", "2_floor", "3_floor", "4_floor" };
            scale = new double[2] { 1, 1 };
            offset = new double[2];
            CurrentFloor = 1;
            location_coordinates = new double[2] { -30, -30 };
            Device.StartTimer(new TimeSpan(0, 0, 5), () =>
            {
                Task.Run(async () =>
                {
                    LocationCoordinates = await LocationTools.GetCurrentLocation();
                });
                return true;
            });
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
        private double rotationAngle;
        private double scaleValue;
        private string[] floorIcon;
        private int currentFloor;
        private double[] offset;
        private double[] location_coordinates;
        private double[] scale;
        private double originYOffset;
        private double[] coordinates;

        #region Properties
        public double RotationAngle { get => rotationAngle + 180 - 46.622711283; set => SetProperty(ref rotationAngle, value); }
        public double ScaleValue { get => scaleValue; set => SetProperty(ref scaleValue, value); }
        public string[] PathIds { get => pathIds; set => SetProperty(ref pathIds, value); }
        public string FloorIcon1 { get => floorIcon[0]; set => SetProperty(ref floorIcon[0], value); }
        public string FloorIcon2 { get => floorIcon[1]; set => SetProperty(ref floorIcon[1], value); }
        public string FloorIcon3 { get => floorIcon[2]; set => SetProperty(ref floorIcon[2], value); }
        public string FloorIcon4 { get => floorIcon[3]; set => SetProperty(ref floorIcon[3], value); }
        public int CurrentFloor { get => currentFloor; set { SetProperty(ref currentFloor, value); FloorChanged(); } }
        public double Offset1 { get => offset[0]; set { SetProperty(ref offset[0], value); } }
        public double Offset2 { get => offset[1]; set { SetProperty(ref offset[1], value); } }
        public double OriginScale { get => scale[0]; set => SetProperty(ref scale[0], value); }
        public double CurrentScale { get => scale[1]; set { SetProperty(ref scale[1], value); RedrawLocation(); } }
        public double OriginYOffset { get => originYOffset; set => SetProperty(ref originYOffset, value); }
        public double LocationX
        {
            get => (location_coordinates[0] * CurrentScale + Offset1) / DeviceDisplay.MainDisplayInfo.Density * OriginScale - 10;
            set => SetProperty(ref location_coordinates[0], value);
        }
        public double LocationY
        {
            get => (location_coordinates[1] * CurrentScale + Offset2) / DeviceDisplay.MainDisplayInfo.Density * OriginScale + (OriginYOffset / DeviceDisplay.MainDisplayInfo.Density / OriginScale) - 10;
            set => SetProperty(ref location_coordinates[1], value);
        }
        public double[] LocationCoordinates { get => coordinates; set { SetProperty(ref coordinates, value); LocationX = coordinates[0]; LocationY = coordinates[1]; } }
        #endregion

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
            RotationAngle = e.Reading.HeadingMagneticNorth;
        }

        private void OpenFlyout()
        {
            Shell.Current.FlyoutIsPresented = true;
        }
        private async void ChoosePath()
        {
            await Shell.Current.GoToAsync(nameof(PathSelectionPage));
        }

        private void UnselectFloor()
        {
            if (FloorIcon1.Contains("glow")) FloorIcon1 = "1_floor";
            if (FloorIcon2.Contains("glow")) FloorIcon2 = "2_floor";
            if (FloorIcon3.Contains("glow")) FloorIcon3 = "3_floor";
            if (FloorIcon4.Contains("glow")) FloorIcon4 = "4_floor";
        }
        private void ChangeFloor(SvgImage image)
        {
            UnselectFloor();
            if (!image.Source.Contains("glow")) image.Source += "_glow";
            if (FloorIcon1.Contains("glow") && FloorIcon1[0] == '1') CurrentFloor = 1;
            if (FloorIcon2.Contains("glow") && FloorIcon2[0] == '2') CurrentFloor = 2;
            if (FloorIcon3.Contains("glow") && FloorIcon3[0] == '3') CurrentFloor = 3;
            if (FloorIcon4.Contains("glow") && FloorIcon4[0] == '4') CurrentFloor = 4;
        }
        private void FloorChanged()
        {
            UnselectFloor();
            if (FloorIcon1.Contains(CurrentFloor.ToString())) FloorIcon1 += "_glow";
            if (FloorIcon2.Contains(CurrentFloor.ToString())) FloorIcon2 += "_glow";
            if (FloorIcon3.Contains(CurrentFloor.ToString())) FloorIcon3 += "_glow";
            if (FloorIcon4.Contains(CurrentFloor.ToString())) FloorIcon4 += "_glow";
        }
        private void RedrawLocation()
        {
            LocationX = location_coordinates[0] + 1; LocationY = location_coordinates[1] + 1;
            LocationX = location_coordinates[0] - 1; LocationY = location_coordinates[1] - 1;
        }

        private void ScaleUp()
        {
            ScaleValue = 2;
            ScaleValue = 0;
        }
        private void ScaleDown()
        {
            ScaleValue = 0.5;
            ScaleValue = 0;
        }

    }
}