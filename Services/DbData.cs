using FlightPlanManager.DataObjects;
using System;
using System.Data.SQLite;

namespace FlightPlanManager.Services
{
    public static class DbData
    {
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
                                Id = rdr.GetInt32(0),
                                Name = (string)rdr[1] ?? string.Empty,
                                Type = (string)rdr[2] ?? string.Empty,
                                Departure = (string)rdr[3] ?? string.Empty,
                                Destination = (string)rdr[4] ?? string.Empty,
                                Distance = (double)rdr.GetDecimal(5),
                                Rating = rdr.GetInt32(6),
                                Group = (string)rdr[7] ?? string.Empty,
                                Notes = (string)rdr[8] ?? string.Empty,
                                Plan = (string)rdr[9] ?? string.Empty,
                                OrigFileName = (string)rdr[10] ?? string.Empty,
                                OrigFullFileName = (string)rdr[11] ?? string.Empty,
                                ImportDate = rdr.GetDateTime(12),
                                FileCreateDate = rdr.IsDBNull(13) ? DateTime.MinValue : rdr.GetDateTime(13),
                                DepartureName = (string)rdr[14] ?? string.Empty,
                                DestinationName = (string)rdr[15] ?? string.Empty
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
                                    DestinationName = rdr[15] as string ?? string.Empty
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
                                    // just eat the error, but load the file.
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
                                           destinationName = @destinationName
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

                    cmd.CommandText = "UPDATE planData SET rating = @rating, groupFlownWith = @group, notes = @notes WHERE id = @id";
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

                    cmd.CommandText = "DELETE FROM planData  WHERE id = @id";
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}