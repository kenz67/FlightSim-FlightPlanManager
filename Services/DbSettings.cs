using FlightPlanManager.DataObjects;
using System.Data.SQLite;

namespace FlightPlanManager.Services
{
    public static class DbSettings
    {
        public static string GetSetting(string setting)
        {
            var val = string.Empty;
            using (var connection = new SQLiteConnection($"Data Source={DbCommon.DbName}"))
            {
                connection.Open();
                using (SQLiteCommand cmd = connection.CreateCommand())
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@key", setting);
                        cmd.CommandText = "SELECT DataValue FROM settings WHERE DataKey = @key";
                        val = cmd.ExecuteScalar().ToString();
                    }
                    catch
                    {
                        val = string.Empty;
                    }
                }
                connection.Close();
            }

            return val;
        }

        public static string SaveSetting(string key, string val)
        {
            using (var connection = new SQLiteConnection($"Data Source={DbCommon.DbName}"))
            {
                connection.Open();

                using (SQLiteCommand cmd = connection.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@key", key);
                    cmd.Parameters.AddWithValue("@val", val);
                    cmd.CommandText = @"INSERT OR REPLACE INTO settings (DataKey, DataValue)
                                        VALUES (@key, @val)";
                    cmd.ExecuteNonQuery();
                }

                connection.Close();
            }

            return val;
        }
    }
}