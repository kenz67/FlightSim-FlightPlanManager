using System;
using System.Device.Location;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FlightPlanManager.Services
{
    public static class GeoCoordinates
    {
        private static bool hasError = false;

        public static bool HasError
        {
            get
            {
                var tmp = hasError;
                hasError = false;
                return tmp;
            }
        }

        public static GeoCoordinate GetGeoCoodinate(string strLatLong)
        {
            var latLongAlt = strLatLong.Split(',');
            var lat = GetValue(latLongAlt[0]);
            var lng = GetValue(latLongAlt[1]);

            return new GeoCoordinate(lat, lng);
        }

        private static double GetValue(string data)
        {
            //strange code to find un-reproducable error.  Maybe related to language settings?
            var step = 0;
            try
            {
                var (direction, value) = GetDirection(data);
                var parts = value.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                var degrees = double.Parse(Regex.Match(parts[step++], @"\d+").Value, new CultureInfo("en-US"));
                var minutes = double.Parse(Regex.Match(parts[step++], @"\d+").Value, new CultureInfo("en-US"));
                var seconds = double.Parse(Regex.Match(parts[step++].Replace(',', '.'), @"[0-9\.]+").Value, new CultureInfo("en-US"));

                return (degrees + (minutes / 60) + (seconds / 3600)) * (direction.Equals("N") || direction.Equals("E") ? 1 : -1);
            }
            catch (Exception ex)
            {
                string txt;
                switch (step - 1)
                {
                    case 0: txt = "degrees"; break;
                    case 1: txt = "minutes"; break;
                    default: txt = "seconds"; break;
                }
                var Logger = NLog.LogManager.GetCurrentClassLogger();
                Logger.Error(ex, $"Error parsing Lat/Long while calculating {txt} for {data}.\n");
                hasError = true;
                return 0;
            }
        }

        private static (string, string) GetDirection(string val)
        {
            return (val.Substring(0, 1), val.Substring(1));
        }
    }
}