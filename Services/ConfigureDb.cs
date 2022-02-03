using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightPlanManager.DataObjects
{
    public class ConfigureDb
    {
        public void InitDb()
        {
            if (!File.Exists(DbCommon.DbName))
            {
                SQLiteConnection.CreateFile(DbCommon.DbName);
            }

            using (var connection = new SQLiteConnection($"Data Source={DbCommon.DbName}"))
            {
                connection.Open();

                CreateSettingTable(connection);
                CreateDataTable(connection);

                LoadSettingTable(connection);
            }
        }

        private void CreateSettingTable(SQLiteConnection conn)
        {
            using (SQLiteCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS settings (
                    DataKey VARCHAR(100) NOT NULL PRIMARY KEY,
                    DataValue VARCHAR(100) NOT NULL
                )";

                cmd.ExecuteNonQuery();
            }
        }

        private void LoadSettingTable(SQLiteConnection conn)
        {
        }

        public void CreateDataTable(SQLiteConnection conn)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS planData (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    planName TEXT,
                    type TEXT,
                    departureId,
                    destinationId,
                    distance DOUBLE,
                    rating INTEGER,
                    groupFlownWith TEXT,
                    notes TEXT,
                    plan TEXT,
                    filename TEXT,
                    fullFileName TEXT,
                    importDate DATETIME
                )";

                cmd.ExecuteNonQuery();
            }
        }
    }
}