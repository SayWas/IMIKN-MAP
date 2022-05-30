using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;


namespace IMIKN_MAP.Services
{
    static class LocationTools
    {
        private static double[] StartCoordinates { get { return new double[2] { 65.522948, 57.158527 }; } }
        private static double[] StartCoordinatesDifference { get { return new double[2] { 22.7, 49.8 }; } }
        private static double ToPixelsCoefficient { get { return 8.032954545454545; } }

        private const double RotationAngle = -(1.57 - 0.75628018189);

        private static double[] ToPixels(double x_meters, double y_meters)
        {
            return new double[2] { (x_meters * ToPixelsCoefficient) + StartCoordinatesDifference[0], (y_meters * ToPixelsCoefficient) + StartCoordinatesDifference[1] };
        }

        public static async Task<double[]> GetCurrentLocation()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(15));
                var location = await Geolocation.GetLocationAsync(request);

                double raw_x_meters = (location.Longitude - StartCoordinates[0]) * 60305.53160034496;
                double raw_y_meters = (location.Latitude - StartCoordinates[1]) * 111251.2902855832;

                double x_meters = (raw_x_meters * Math.Sin(RotationAngle)) + (raw_y_meters * Math.Cos(RotationAngle));
                double y_meters = (raw_x_meters * Math.Cos(RotationAngle)) - (raw_y_meters * Math.Sin(RotationAngle));

                return ToPixels(x_meters, y_meters);
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
                return null;
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
                return null;
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
                return null;
            }
            catch (Exception ex)
            {
                // Unable to get location
                return null;
            }
        }
    }
}
