using FlightPlanManager.DataObjects;
using System;
using System.Data.SQLite;

namespace FlightPlanManager.Services
{
    public static class DbData
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static DbPlanObject GetData(int id)
        {
            var result = new DbPlanObject();

            using (var connection = new SQLiteConnection($"Data Source={DbCommon.DbName}"))
            {
                connection.Open();

                using (SQLiteCommand cmd = connection.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.CommandText = @"SELECT
                               [id]
                              ,[planName]
                              ,[type]
                              ,[departureId]
                              ,[destinationId]
                              ,[distance]
                              ,[rating]
                              ,[groupFlownWith]
                              ,[notes]
                              ,[plan]
                              ,[filename]
                              ,[fullFileName]
                              ,[importDate]
                              ,[fileCreateDate]
                              ,[departureName]
                              ,[destinationName]
                        FROM [planData]
                        WHERE id = @id";

                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            result = new DbPlanObject
                            {
                                Id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0),
                                Name = rdr.IsDBNull(1) ? string.Empty : rdr.GetString(1),
                                Type = rdr.IsDBNull(2) ? string.Empty : rdr.GetString(2),
                                Departure = rdr.IsDBNull(3) ? string.Empty : rdr.GetString(3),
                                Destination = rdr.IsDBNull(4) ? string.Empty : rdr.GetString(4),
                                Distance = rdr.IsDBNull(5) ? 0 : (double)rdr.GetDecimal(5),
                                Rating = rdr.IsDBNull(6) ? 0 : rdr.GetInt32(6),
                                Group = rdr.IsDBNull(7) ? string.Empty : rdr.GetString(7),
                                Notes = rdr.IsDBNull(8) ? string.Empty : rdr.GetString(8),
                                Plan = rdr.IsDBNull(9) ? string.Empty : rdr.GetString(9),
                                OrigFileName = rdr.IsDBNull(10) ? string.Empty : rdr.GetString(10),
                                OrigFullFileName = rdr.IsDBNull(11) ? string.Empty : rdr.GetString(11),
                                ImportDate = rdr.IsDBNull(12) ? DateTime.MinValue : rdr.GetDateTime(12),
                                FileCreateDate = rdr.IsDBNull(13) ? DateTime.MinValue : rdr.GetDateTime(13),
                                DepartureName = rdr.IsDBNull(14) ? string.Empty : rdr.GetString(14),
                                DestinationName = rdr.IsDBNull(15) ? string.Empty : rdr.GetString(15)
                            };
                        }
                    }
                }
            }
            return result;
        }

        public static SortableBindingList<DbPlanObject> GetData()
        {
            var result = new SortableBindingList<DbPlanObject>();
            using (var connection = new SQLiteConnection($"Data Source={DbCommon.DbName}"))
            {
                connection.Open();

                using (SQLiteCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"SELECT
                               [id]
                              ,[planName]
                              ,[type]
                              ,[departureId]
                              ,[destinationId]
                              ,[distance]
                              ,[rating]
                              ,[groupFlownWith]
                              ,[notes]
                              ,[plan]
                              ,[filename]
                              ,[fullFileName]
                              ,[importDate]
                              ,[FileCreateDate]
                              ,[departureName]
                              ,[destinationName]
                              ,[author]
                              ,[airportCnt]
                              ,[waypointCnt]
                        FROM [planData]
                        ORDER BY importDate DESC";

                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            try
                            {
                                result.Add(new DbPlanObject
                                {
                                    Id = rdr.GetInt32(0),
                                    Name = rdr[1] as string,
                                    Type = rdr[2] as string,
                                    Departure = rdr[3] as string,
                                    Destination = rdr[4] as string,
                                    Distance = (double)rdr.GetDecimal(5),
                                    Rating = rdr.GetInt32(6),
                                    Group = rdr[7] as string,
                                    Notes = rdr[8] as string,
                                    Plan = rdr[9] as string,
                                    OrigFileName = rdr[10] as string,
                                    OrigFullFileName = rdr[11] as string,
                                    ImportDate = rdr.GetDateTime(12),
                                    FileCreateDate = rdr.IsDBNull(13) ? DateTime.MinValue : rdr.GetDateTime(13),
                                    DepartureName = rdr[14] as string ?? string.Empty,
                                    DestinationName = rdr[15] as string ?? string.Empty,
                                    Author = rdr[16] as string ?? string.Empty,
                                    AirportCount = rdr.IsDBNull(17) ? 0 : rdr.GetInt32(17),
                                    WaypointCount = rdr.IsDBNull(18) ? 0 : rdr.GetInt32(18)
                                });
                            }
                            catch (Exception ex)
                            {
                                try
                                {
                                    result.Add(new DbPlanObject
                                    {
                                        Id = rdr.GetInt32(0),
                                        Name = rdr[1] as string,
                                        Notes = "Error reading record from Db"
                                    });
                                }
                                catch
                                {
                                    Logger.Error(ex, "Reading Data");
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        public static int AddPlan(DbPlanObject plan)
        {
            int id;
            using (var connection = new SQLiteConnection($"Data Source={DbCommon.DbName}"))
            {
                connection.Open();

                using (SQLiteCommand cmd = connection.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@name", plan.Name);
                    cmd.Parameters.AddWithValue("@type", plan.Type);
                    cmd.Parameters.AddWithValue("@departureId", plan.Departure);
                    cmd.Parameters.AddWithValue("@destinationId", plan.Destination);
                    cmd.Parameters.AddWithValue("@distance", plan.Distance);
                    cmd.Parameters.AddWithValue("@rating", plan.Rating);
                    cmd.Parameters.AddWithValue("@groupFlownWith", plan.Group);
                    cmd.Parameters.AddWithValue("@notes", plan.Notes);
                    cmd.Parameters.AddWithValue("@plan", plan.Plan);
                    cmd.Parameters.AddWithValue("@filename", plan.OrigFileName);
                    cmd.Parameters.AddWithValue("@fullFileName", plan.OrigFullFileName);
                    cmd.Parameters.AddWithValue("@importDate", plan.ImportDate);
                    cmd.Parameters.AddWithValue("@fileCreateDate", plan.FileCreateDate);
                    cmd.Parameters.AddWithValue("@departureName", plan.DepartureName);
                    cmd.Parameters.AddWithValue("@destinationName", plan.DestinationName);

                    cmd.CommandText = "SELECT planName FROM planData WHERE planName = @name AND filename = @filename";
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            return -1;
                        }
                    }

                    cmd.CommandText = @"INSERT INTO planData (
                                       planName,
                                       type,
                                       departureId,
                                       destinationId,
                                       distance,
                                       rating,
                                       groupFlownWith,
                                       notes,
                                       plan,
                                       filename,
                                       fullFileName,
                                       importDate,
                                       fileCreateDate,
                                       destinationName,
                                       departureName
                                        )
                                     VALUES (
                                        @name,
                                        @type,
                                        @departureId,
                                        @destinationId,
                                        @distance,
                                        @rating,
                                        @groupFlownWith,
                                        @notes,
                                        @plan,
                                        @filename,
                                        @fullFileName,
                                        @importDate,
                                        @fileCreateDate,
                                        @departureName,
                                        @destinationName
                                    )";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "SELECT last_insert_rowid()";
                    object tmp = cmd.ExecuteScalar();
                    id = int.Parse(tmp.ToString());
                }

                connection.Close();
            }

            return id;
        }

        public static int OverwritePlan(DbPlanObject plan)
        {
            int id;
            using (var connection = new SQLiteConnection($"Data Source={DbCommon.DbName}"))
            {
                connection.Open();

                using (SQLiteCommand cmd = connection.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@name", plan.Name);
                    cmd.Parameters.AddWithValue("@type", plan.Type);
                    cmd.Parameters.AddWithValue("@departureId", plan.Departure);
                    cmd.Parameters.AddWithValue("@destinationId", plan.Destination);
                    cmd.Parameters.AddWithValue("@distance", plan.Distance);
                    cmd.Parameters.AddWithValue("@plan", plan.Plan);
                    cmd.Parameters.AddWithValue("@filename", plan.OrigFileName);
                    cmd.Parameters.AddWithValue("@fullFileName", plan.OrigFullFileName);
                    cmd.Parameters.AddWithValue("@importDate", plan.ImportDate);
                    cmd.Parameters.AddWithValue("@fileCreateDate", plan.FileCreateDate);
                    cmd.Parameters.AddWithValue("@departureName", plan.DepartureName);
                    cmd.Parameters.AddWithValue("@destinationName", plan.DestinationName);
                    cmd.Parameters.AddWithValue("@author", plan.Author);
                    cmd.Parameters.AddWithValue("@airportCount", plan.AirportCount);
                    cmd.Parameters.AddWithValue("@waypointCount", plan.WaypointCount);

                    cmd.CommandText = @"UPDATE planData SET
                                           planName = @name,
                                           type = @type,
                                           departureId = @departureId,
                                           destinationId = @destinationId,
                                           distance = @distance,
                                           plan = @plan,
                                           filename = @filename,
                                           fullFileName = @fullFileName,
                                           importDate = @importDate,
                                           fileCreateDate = @fileCreateDate,
                                           departureName = @departureName,
                                           destinationName = @destinationName,
                                           author = @author,
                                           airportCnt = @airportCount,
                                           waypointCnt = @waypointCount
                                        WHERE planName = @name AND filename = @filename";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "SELECT last_insert_rowid()";
                    object tmp = cmd.ExecuteScalar();
                    id = int.Parse(tmp.ToString());
                }

                connection.Close();
            }

            return id;
        }

        public static void Update(DbPlanObject data)
        {
            using (var connection = new SQLiteConnection($"Data Source={DbCommon.DbName}"))
            {
                connection.Open();

                using (SQLiteCommand cmd = connection.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@id", data.Id);
                    cmd.Parameters.AddWithValue("@rating", data.Rating);
                    cmd.Parameters.AddWithValue("@group", data.Group);
                    cmd.Parameters.AddWithValue("@notes", data.Notes);
                    cmd.Parameters.AddWithValue("@author", data.Author);

                    cmd.CommandText = "UPDATE planData SET rating = @rating, groupFlownWith = @group, notes = @notes, author = @author WHERE id = @id";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void Delete(int id)
        {
            using (var connection = new SQLiteConnection($"Data Source={DbCommon.DbName}"))
            {
                connection.Open();

                using (SQLiteCommand cmd = connection.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    cmd.CommandText = "DELETE FROM planData WHERE id = @id";
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}