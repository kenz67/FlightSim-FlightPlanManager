using System;

namespace FlightPlanManager.DataObjects
{
    public static class DbCommon
    {
        public static readonly string DbName = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\FlightPlanManager\\FlightPlanManager.sqlite";
        public static readonly string SettingsDefaultFolder = "DefaultFolder";
        public static readonly string SettingsWindowPosition = "WindowPosition";
        public static readonly string SettingsMapWindowPosition = "MapWindowPosition";
    }
}