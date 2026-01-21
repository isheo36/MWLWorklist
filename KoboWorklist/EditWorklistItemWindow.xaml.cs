using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using KoboWorklist.WorklistSCP.Model;

namespace KoboWorklist
{
    public partial class EditWorklistItemWindow : Window
    {
        public WorklistItem WorklistItem { get; private set; }

        public EditWorklistItemWindow(WorklistItem item)
        {
            InitializeComponent();
            WorklistItem = item;

            // Populate fields with the item's data
            AccessionNumberTextBox.Text = WorklistItem.AccessionNumber;
            PatientIDTextBox.Text = WorklistItem.PatientID;
            SurnameTextBox.Text = WorklistItem.Surname;
            ForenameTextBox.Text = WorklistItem.Forename;
            DateOfBirthTextBox.Text = WorklistItem.DateOfBirth.ToString("yyyy-MM-dd");
            ExamDescriptionTextBox.Text = WorklistItem.ExamDescription;
            StudyUIDTextBox.Text = WorklistItem.StudyUID;
            ScheduledAETTextBox.Text = WorklistItem.ScheduledAET;
            ReferringPhysicianTextBox.Text = WorklistItem.ReferringPhysician;
            ExamDateAndTimeTextBox.Text = WorklistItem.ExamDateAndTime.ToString("yyyy-MM-dd HH:mm:ss");

            // Populate Modality ComboBox
            ModalityComboBox.ItemsSource = App.Modalities;
            ModalityComboBox.SelectedItem = WorklistItem.Modality;


            SexComboBox.ItemsSource = new[]
            {
                new { Display = "Male", Value = "M" },
                new { Display = "Female", Value = "F" },
                new { Display = "Other", Value = "O" }
            };

            SexComboBox.DisplayMemberPath = "Display";
            SexComboBox.SelectedValuePath = "Value";
            SexComboBox.SelectedValue = WorklistItem.Sex;

            // Add event handler for PatientID changes
            PatientIDTextBox.TextChanged += PatientIDTextBox_TextChanged;
        }

        private void PatientIDTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string patientId = PatientIDTextBox.Text;

            if (IsValidBulgarianEGN(patientId, out DateTime birthDate) || IsValidBulgarianLNCH(patientId, out birthDate))
            {
                DateOfBirthTextBox.Text = birthDate.ToString("yyyy-MM-dd");

                // Extract and set the sex based on the 9th digit
                char sexDigit = patientId[8];
                if (sexDigit % 2 == 0) // Even digit
                {
                    SexComboBox.SelectedValue = "M"; // Male
                }
                else // Odd digit
                {
                    SexComboBox.SelectedValue = "F"; // Female
                }
            }
        }

        private bool IsValidBulgarianEGN(string egn, out DateTime birthDate)
        {
            birthDate = default;

            if (egn.Length != 10 || !Regex.IsMatch(egn, @"^\d{10}$"))
                return false;

            string yearPrefix = egn.Substring(0, 2);
            string month = egn.Substring(2, 2);
            string day = egn.Substring(4, 2);

            int year = int.Parse(yearPrefix);
            int monthInt = int.Parse(month);

            if (monthInt >= 1 && monthInt <= 12)
            {
                year += 1900;
            }
            else if (monthInt >= 21 && monthInt <= 32)
            {
                year += 1800;
                monthInt -= 20;
            }
            else if (monthInt >= 41 && monthInt <= 52)
            {
                year += 2000;
                monthInt -= 40;
            }
            else
            {
                return false;
            }

            if (DateTime.TryParseExact($"{year}-{monthInt:D2}-{day}", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out birthDate))
            {
                return true;
            }

            return false;
        }

        private bool IsValidBulgarianLNCH(string lnch, out DateTime birthDate)
        {
            birthDate = default;

            if (lnch.Length != 10 || !Regex.IsMatch(lnch, @"^\d{10}$"))
                return false;

            string yearPrefix = lnch.Substring(0, 2);
            string month = lnch.Substring(2, 2);
            string day = lnch.Substring(4, 2);

            int year = int.Parse(yearPrefix);
            int monthInt = int.Parse(month);

            if (monthInt >= 1 && monthInt <= 12)
            {
                year += 1900;
            }
            else
            {
                return false;
            }

            if (DateTime.TryParseExact($"{year}-{monthInt:D2}-{day}", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out birthDate))
            {
                return true;
            }

            return false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Update the WorklistItem with the edited values
                WorklistItem.AccessionNumber = AccessionNumberTextBox.Text;
                WorklistItem.PatientID = PatientIDTextBox.Text;
                WorklistItem.Surname = SurnameTextBox.Text;
                WorklistItem.Forename = ForenameTextBox.Text;
                WorklistItem.Sex = SexComboBox.SelectedValue?.ToString();
                WorklistItem.DateOfBirth = DateTime.Parse(DateOfBirthTextBox.Text);
                WorklistItem.Modality = ModalityComboBox.SelectedItem?.ToString();
                WorklistItem.ExamDescription = ExamDescriptionTextBox.Text;
                WorklistItem.StudyUID = StudyUIDTextBox.Text;
                WorklistItem.ScheduledAET = ScheduledAETTextBox.Text;
                WorklistItem.ReferringPhysician = ReferringPhysicianTextBox.Text;
                WorklistItem.ExamDateAndTime = DateTime.Parse(ExamDateAndTimeTextBox.Text);

                DialogResult = true; // Indicate success
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving worklist item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Indicate cancellation
            Close();
        }
    }
}