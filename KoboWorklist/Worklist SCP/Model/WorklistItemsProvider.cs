// Copyright (c) 2012-2025 fo-dicom contributors.
// Licensed under the Microsoft Public License (MS-PL).

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace KoboWorklist.WorklistSCP.Model
{
    public class WorklistItemsProvider : IWorklistItemsSource
    {
        private readonly string _databasePath;
        private readonly IConfiguration _configuration;

        public WorklistItemsProvider(string databasePath, IConfiguration configuration)
        {
            _databasePath = databasePath;
            _configuration = configuration;
        }

        public List<WorklistItem> GetAllCurrentWorklistItems()
        {
            var worklistItems = new List<WorklistItem>();

            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM WorklistItems";
            using var reader = command.ExecuteReader();

            // Load base UUID from configuration
            var baseUuid = _configuration.GetValue<string>("BaseUUID", "1.2.840.113619"); // Default base UUID

            while (reader.Read())
            {
                var id = int.Parse(reader["Id"].ToString());
                var currentYear = DateTime.Now.Year;
                long timestampInMicroseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                var item = new WorklistItem
                {
                    Id = id,
                    AccessionNumber = reader["AccessionNumber"].ToString(),
                    DateOfBirth = DateTime.Parse(reader["DateOfBirth"].ToString()),
                    PatientID = reader["PatientID"].ToString(),
                    Surname = reader["Surname"].ToString(),
                    Forename = reader["Forename"].ToString(),
                    Sex = reader["Sex"].ToString(),
                    Title = reader["Title"].ToString(),
                    Modality = reader["Modality"].ToString(),
                    ExamDescription = reader["ExamDescription"].ToString(),
                    ExamRoom = reader["ExamRoom"].ToString(),
                    HospitalName = reader["HospitalName"].ToString(),
                    PerformingPhysician = reader["PerformingPhysician"].ToString(),
                    ProcedureID = (10000 + id).ToString(),
                    ProcedureStepID = (20000 + id).ToString(),
                    StudyUID = $"{baseUuid}.{currentYear}.{timestampInMicroseconds}.{id}.1", // Generate StudyUID
                    ScheduledAET = reader["ScheduledAET"].ToString(),
                    ReferringPhysician = reader["ReferringPhysician"].ToString(),
                    ExamDateAndTime = DateTime.Parse(reader["ExamDateAndTime"].ToString())
                };

                worklistItems.Add(item);
            }

            return worklistItems;
        }

        public void AddWorklistItem(WorklistItem item)
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO WorklistItems (AccessionNumber, DateOfBirth, PatientID, Surname, Forename, Sex, Modality, ExamDescription, StudyUID, ScheduledAET, ReferringPhysician, ExamDateAndTime)
                VALUES ($AccessionNumber, $DateOfBirth, $PatientID, $Surname, $Forename, $Sex, $Modality, $ExamDescription, $StudyUID, $ScheduledAET, $ReferringPhysician, $ExamDateAndTime)";
            command.Parameters.AddWithValue("$AccessionNumber", item.AccessionNumber);
            command.Parameters.AddWithValue("$DateOfBirth", item.DateOfBirth);
            command.Parameters.AddWithValue("$PatientID", item.PatientID);
            command.Parameters.AddWithValue("$Surname", item.Surname);
            command.Parameters.AddWithValue("$Forename", item.Forename);
            command.Parameters.AddWithValue("$Sex", item.Sex);
            command.Parameters.AddWithValue("$Modality", item.Modality);
            command.Parameters.AddWithValue("$ExamDescription", item.ExamDescription);
            command.Parameters.AddWithValue("$StudyUID", item.StudyUID);
            command.Parameters.AddWithValue("$ScheduledAET", item.ScheduledAET);
            command.Parameters.AddWithValue("$ReferringPhysician", item.ReferringPhysician);
            command.Parameters.AddWithValue("$ExamDateAndTime", item.ExamDateAndTime);
            command.ExecuteNonQuery();
        }

        public void UpdateWorklistItem(WorklistItem item)
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE WorklistItems
                SET AccessionNumber = $AccessionNumber, DateOfBirth = $DateOfBirth, PatientID = $PatientID, Surname = $Surname, Forename = $Forename, Sex = $Sex, Modality = $Modality, ExamDescription = $ExamDescription, StudyUID = $StudyUID, ScheduledAET = $ScheduledAET, ReferringPhysician = $ReferringPhysician, ExamDateAndTime = $ExamDateAndTime
                WHERE id = $Id";
            command.Parameters.AddWithValue("$Id", item.Id);
            command.Parameters.AddWithValue("$AccessionNumber", item.AccessionNumber);
            command.Parameters.AddWithValue("$DateOfBirth", item.DateOfBirth);
            command.Parameters.AddWithValue("$PatientID", item.PatientID);
            command.Parameters.AddWithValue("$Surname", item.Surname);
            command.Parameters.AddWithValue("$Forename", item.Forename);
            command.Parameters.AddWithValue("$Sex", item.Sex);
            command.Parameters.AddWithValue("$Modality", item.Modality);
            command.Parameters.AddWithValue("$ExamDescription", item.ExamDescription);
            command.Parameters.AddWithValue("$StudyUID", item.StudyUID);
            command.Parameters.AddWithValue("$ScheduledAET", item.ScheduledAET);
            command.Parameters.AddWithValue("$ReferringPhysician", item.ReferringPhysician);
            command.Parameters.AddWithValue("$ExamDateAndTime", item.ExamDateAndTime);
            command.ExecuteNonQuery();
        }

        public void DeleteWorklistItem(WorklistItem item)
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM WorklistItems WHERE Id = $Id";
            command.Parameters.AddWithValue("$Id", item.Id);
            command.ExecuteNonQuery();
        }
    }
}
