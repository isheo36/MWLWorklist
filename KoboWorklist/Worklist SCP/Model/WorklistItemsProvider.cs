// Copyright (c) 2012-2025 fo-dicom contributors.
// Licensed under the Microsoft Public License (MS-PL).

using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace KoboWorklist.WorklistSCP.Model
{
    public class WorklistItemsProvider : IWorklistItemsSource
    {
        private readonly string _databasePath;

        public WorklistItemsProvider(string databasePath)
        {
            _databasePath = databasePath;
        }

        public List<WorklistItem> GetAllCurrentWorklistItems()
        {
            var worklistItems = new List<WorklistItem>();

            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM WorklistItems";
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var item = new WorklistItem
                {
                    Id = int.Parse(reader["Id"].ToString()),
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
                    //ProcedureID = reader["ProcedureID"].ToString(),
                    //ProcedureStepID = reader["ProcedureStepID"].ToString(),
                    ProcedureID = (10000 + int.Parse(reader["Id"].ToString())).ToString(),
                    ProcedureStepID = (20000 + int.Parse(reader["Id"].ToString())).ToString(),
                    StudyUID = reader["StudyUID"].ToString(),
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
