using FlightPlanManager.DataObjects;
using FlightPlanManager.Models;
using System.Collections.Generic;
using System.Data.SQLite;

namespace FlightPlanManager.Services
{
    public static class DbColumns
    {
        public static List<DbGridColumn> GetData()
        {
            var result = new List<DbGridColumn>();

            using (var connection = new SQLiteConnection($"Data Source={DbCommon.DbName}"))
            {
                connection.Open();

                using (SQLiteCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"SELECT
                               [ColumnKey]
                              ,[ColumnName]
                              ,[DisplayOrder]
                        FROM [gridColumns]
                        ORDER BY DisplayOrder, ColumnName";

                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            result.Add(new DbGridColumn
                            {
                                ColumnKey = (string)rdr[0] ?? string.Empty,
                                ColumnName = (string)rdr[1] ?? string.Empty,
                                DisplayOrder = rdr.GetInt16(2),
                            });
                        }
                    }
                }
            }
            return result;
        }

        public static void SaveData(List<DbGridColumn> data)
        {
            using (var connection = new SQLiteConnection($"Data Source={DbCommon.DbName}"))
            {
                connection.Open();

                using (SQLiteCommand cmd = connection.CreateCommand())
                {
                    foreach (var row in data)
                    {
                        cmd.Parameters.AddWithValue("@key", row.ColumnKey);
                        cmd.Parameters.AddWithValue("@order", row.DisplayOrder);

                        cmd.CommandText = "UPDATE gridColumns SET DisplayOrder = @order WHERE ColumnKey = @key";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}