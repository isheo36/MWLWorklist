using Microsoft.Data.Sqlite;
using System.IO;

namespace KoboWorklist.WorklistSCP.Model
{
    public static class DatabaseInitializer
    {
        public static void InitializeDatabase(string databasePath)
        {
            if (!File.Exists(databasePath))
            {
                using var connection = new SqliteConnection($"Data Source={databasePath}");
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE WorklistItems (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        AccessionNumber TEXT,
                        DateOfBirth TEXT,
                        PatientID TEXT,
                        Surname TEXT,
                        Forename TEXT,
                        Sex TEXT,
                        Title TEXT,
                        Modality TEXT,
                        ExamDescription TEXT,
                        ExamRoom TEXT,
                        HospitalName TEXT,
                        PerformingPhysician TEXT,
                        ProcedureID TEXT,
                        ProcedureStepID TEXT,
                        StudyUID TEXT,
                        ScheduledAET TEXT,
                        ReferringPhysician TEXT,
                        ExamDateAndTime TEXT
                    )";
                command.ExecuteNonQuery();
            }
        }
    }
}