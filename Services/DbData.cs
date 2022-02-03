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
        public static SortableBindingList<DbPlanObject> GetData()
        {
            return new SortableBindingList<DbPlanObject>
            {
                new DbPlanObject { Id = 0, Name = "one", ImportDate = DateTime.Now, Type = "VFR", Departure = "KJFK", Destination = "KORD", Rating = 1, Group = "group 1", Notes = "hi", Distance = 50 },
                new DbPlanObject { Id = 1, Name = "two", ImportDate = DateTime.Now, Type = "IFR", Rating = 3, Group = "solo", Notes = "bye", Distance = 200 },
                new DbPlanObject { Id = 2, Name = "three", ImportDate = DateTime.Now, Type = "VFR", Rating = 2, Group = "solo", Notes = "hello", Distance = 150 }
            };
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

                    cmd.CommandText = "SELECT Name FROM planData WHERE planName = @name";
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
                                        @groupFlownWith
                                        @notes,
                                        @plan,
                                        @filename,
                                        @fulFileName,
                                        @importDate)";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "SELECT last_insert_rowid()";
                    id = (int)cmd.ExecuteScalar();
                }

                connection.Close();
            }

            return id;
        }
    }
}