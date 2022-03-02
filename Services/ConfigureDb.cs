using System;
using System.Data.SQLite;
using System.IO;

namespace FlightPlanManager.DataObjects
{
    public class ConfigureDb
    {
        public void InitDb()
        {
            if (!File.Exists(DbCommon.DbName))
            {
                var path = Path.GetDirectoryName(System.IO.Path.GetFullPath(DbCommon.DbName));
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                SQLiteConnection.CreateFile(DbCommon.DbName);
            }

            using (var connection = new SQLiteConnection($"Data Source={DbCommon.DbName}"))
            {
                connection.Open();

                CreateSettingTable(connection);
                LoadSettingTable(connection);

                CreateDataTable(connection);
                CreateGridColumnsTable(connection);
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
                    SELECT 'notesDataGridViewTextBoxColumn', 'Notes', 9 UNION
                    SELECT 'fileCreateDateDataTextBoxColumn', 'File Create Date', 10 UNION
                    SELECT 'DestinationNameTextBoxColumn', 'Dest Name', 11 UNION
                    SELECT 'DepartureNameTextBoxColumn', 'Dep Name', 12 UNION
                    SELECT 'AuthorDataGridViewTextBoxColumn', 'Author', 13 UNION
                    SELECT 'AirportCountDataGridViewTextBoxColumn', 'Airport Count', 14 UNION
                    SELECT 'WaypointCountDataGridViewTextBoxColumn', 'Waypoint Count', 15
                )
                INSERT INTO gridColumns (ColumnKey, ColumnName, DisplayOrder)
                    SELECT ColumnKey, ColumnName, DisplayOrder FROM v t1
                    WHERE NOT EXISTS (SELECT 1 FROM gridColumns t2 WHERE t1.ColumnKey = t2.ColumnKey);
            ";

                cmd.ExecuteNonQuery();
            }
        }

        private void LoadSettingTable(SQLiteConnection conn)
        {
            using (SQLiteCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = $@"
                WITH v AS (
	                SELECT '{DbCommon.SettingsDefaultFolder}' as DataKey, 'C:\\Users\\{ Environment.UserName}\\AppData\\Local\\Packages\\Microsoft.FlightSimulator_8wekyb3d8bbwe\\LocalState' as DataValue UNION
                    SELECT '{DbCommon.SettingsOverwrite}', 'True'
                )
                INSERT INTO settings (DataKey, DataValue)
                    SELECT DataKey, DataValue FROM v t1
                    WHERE NOT EXISTS (SELECT 1 FROM settings t2 WHERE t1.DataKey = t2.DataKey);
            ";

                cmd.ExecuteNonQuery();
            }
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
                    importDate DATETIME,
                    fileCreateDate DATETIME,
                    departureName TEXT,
                    destinationName TEXT,
                    author TEXT,
                    airportCnt INTEGER,
                    waypointCnt INTEGER
                )";

                cmd.ExecuteNonQuery();
            }

            AddColumn(conn, "planData", "fileCreateDate");
            AddColumn(conn, "planData", "departureName");
            AddColumn(conn, "planData", "destinationName");
            AddColumn(conn, "planData", "author");
            AddIntColumn(conn, "planData", "airportCnt");
            AddIntColumn(conn, "planData", "waypointCnt");
        }

        public void AddColumn(SQLiteConnection conn, string table, string col)
        {
            if (!CheckColumnExists(conn, table, col))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@table", table);
                    cmd.Parameters.AddWithValue("@col", col);
                    cmd.CommandText = $"ALTER TABLE {table} ADD {col} TEXT NULL";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void AddIntColumn(SQLiteConnection conn, string table, string col)
        {
            if (!CheckColumnExists(conn, table, col))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@table", table);
                    cmd.Parameters.AddWithValue("@col", col);
                    cmd.CommandText = $"ALTER TABLE {table} ADD {col} INTEGER NULL";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool CheckColumnExists(SQLiteConnection conn, string table, string col)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@table", table);
                cmd.Parameters.AddWithValue("@col", col);
                cmd.CommandText = "SELECT COUNT(*) AS CNTREC FROM pragma_table_info(@table) WHERE name=@col";
                var result = Convert.ToInt16(cmd.ExecuteScalar());
                return result.Equals(1);
            }
        }
    }
}