using FlightPlanManager.DataObjects;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    cmd.Parameters.AddWithValue("id", id);
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
                        FROM [planData]
                        WHERE id = @id";

                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            result = new DbPlanObject
                            {
                                Id = rdr.GetInt32(0),
                                Name = rdr.GetString(1),
                                Type = rdr.GetString(2),
                                Departure = rdr.GetString(3),
                                Destination = rdr.GetString(4),
                                Distance = (double)rdr.GetDecimal(5),
                                Rating = rdr.GetInt32(6),
                                Group = rdr.GetString(7),
                                Notes = rdr.GetString(8),
                                Plan = rdr.GetString(9),
                                OrigFileName = rdr.GetString(10),
                                OrigFullFileName = rdr.GetString(11),
                                ImportDate = rdr.GetDateTime(12)
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
            {
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
                        FROM [planData]
                        ORDER BY importDate DESC";

                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                result.Add(new DbPlanObject
                                {
                                    Id = rdr.GetInt32(0),
                                    Name = rdr.GetString(1),
                                    Type = rdr.GetString(2),
                                    Departure = rdr.GetString(3),
                                    Destination = rdr.GetString(4),
                                    Distance = (double)rdr.GetDecimal(5),
                                    Rating = rdr.GetInt32(6),
                                    Group = rdr.GetString(7),
                                    Notes = rdr.GetString(8),
                                    Plan = rdr.GetString(9),
                                    OrigFileName = rdr.GetString(10),
                                    OrigFullFileName = rdr.GetString(11),
                                    ImportDate = rdr.GetDateTime(12)
                                });
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
                    cmd.Parameters.AddWithValue("@fulFileName", plan.OrigFullFileName);
                    cmd.Parameters.AddWithValue("@importDate", plan.ImportDate);

                    cmd.CommandText = "SELECT planName FROM planData WHERE planName = @name";
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
                                       importDate)
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
                                        @fulFileName,
                                        @importDate)";
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