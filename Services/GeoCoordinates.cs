using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FlightPlanManager.Services
{
    public static class GeoCoordinates
    {
        public static GeoCoordinate GetGeoCoodinate(string strLatLong)
        {
            var latLongAlt = strLatLong.Split(',');
            var lat = GetValue(latLongAlt[0]);
            var lng = GetValue(latLongAlt[1]);

            return new GeoCoordinate(lat, lng);
        }

        private static double GetValue(string data)
        {
            var (direction, value) = GetDirection(data);
            var parts = value.Split(' ');
            var degrees = double.Parse(Regex.Match(parts[0], @"\d+").Value);
            var minutes = double.Parse(Regex.Match(parts[1], @"\d+").Value);
            var seconds = double.Parse(Regex.Match(parts[2], @"[0-9\.]+").Value);

            return (degrees + (minutes / 60) + (seconds / 3600)) * (direction.Equals("N") || direction.Equals("E") ? 1 : -1);
        }

        private static (string, string) GetDirection(string val)
        {
            return (val.Substring(0, 1), val.Substring(1));
        }
    }
}