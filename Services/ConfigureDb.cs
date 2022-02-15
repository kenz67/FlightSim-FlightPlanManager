using FlightPlanManager.Services;
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
                CreateGridColumnsTable(connection);

                LoadSettingTable();
                LoadGridColumnsTable(connection);
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

        private void CreateGridColumnsTable(SQLiteConnection conn)
        {
            using (SQLiteCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS gridColumns (
                    ColumnKey VARCHAR(100) NOT NULL PRIMARY KEY,
                    ColumnName VARCHAR(100) NOT NULL,
                    DisplayOrder INT NOT NULL
                )";

                cmd.ExecuteNonQuery();
            }
        }

        //   create constants
        //   SELECT '{SettingDefinitions.ApplyFuel}', 'true' UNION
        private void LoadGridColumnsTable(SQLiteConnection conn)
        {
            using (SQLiteCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                WITH v AS (
	                SELECT 'nameDataGridViewTextBoxColumn' as ColumnKey, 'Name' as ColumnName, 0 as DisplayOrder UNION
                    SELECT 'typeDataGridViewTextBoxColumn', 'Type', 1 UNION
                    SELECT 'importDateDataGridViewTextBoxColumn', 'Import Date', 2 UNION
                    SELECT 'departureDataGridViewTextBoxColumn', 'Departure', 3 UNION
                    SELECT 'destinationDataGridViewTextBoxColumn', 'Destination', 4 UNION
                    SELECT 'ratingDataGridViewTextBoxColumn', 'Rating', 5 UNION
                    SELECT 'distanceDataGridViewTextBoxColumn', 'Distance', 6 UNION
                    SELECT 'groupDataGridViewTextBoxColumn', 'Group', 7 UNION
                    SELECT 'origFileNameDataGridViewTextBoxColumn', 'Orig File Name', 8 UNION
                    SELECT 'notesDataGridViewTextBoxColumn', 'Notes', 9
                )
                INSERT INTO gridColumns (ColumnKey, ColumnName, DisplayOrder)
                    SELECT ColumnKey, ColumnName, DisplayOrder FROM v t1
                    WHERE NOT EXISTS (SELECT 1 FROM gridColumns t2 WHERE t1.ColumnKey = t2.ColumnKey);
            ";

                cmd.ExecuteNonQuery();
            }
        }

        private void LoadSettingTable()
        {
            DbSettings.SaveSetting(DbCommon.SettingsDefaultFolder, $"C:\\Users\\{Environment.UserName}\\AppData\\Local\\Packages\\Microsoft.FlightSimulator_8wekyb3d8bbwe\\LocalState");
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