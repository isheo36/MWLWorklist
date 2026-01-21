using FellowOakDicom;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
using KoboWorklist.WorklistSCP;
using KoboWorklist.WorklistSCP.Model;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Configuration.FileExtensions;
//using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Timers;
using System.Windows;
using Timer = System.Timers.Timer;

namespace KoboWorklist
{
    public partial class App : Application
    {
        Timer _timer = new Timer(30000);
        private static readonly ILog log = LogManager.GetLogger(typeof(App));
        private const string DatabasePath = "WorklistItems.db";

        public static List<string> Modalities { get; private set; }

        private bool checkPacs = false; // Default: true
        private string pacsIp = "127.0.0.1"; // Default: 127.0.0.1
        private int pacsPort = 9104; // Default: 9104
        private string pacsAET = "PACS"; // Default: STOR
        private string localAET = "WORKLIST"; // Default: KOBOWORKLIST

        protected override void OnStartup(StartupEventArgs e)
        {

            base.OnStartup(e);

            // Load configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Load modalities from configuration
            Modalities = configuration.GetSection("Modalities").Get<List<string>>();

            // Initialize the database
            DatabaseInitializer.InitializeDatabase(DatabasePath);

            // Optionally populate the database with sample data
            PopulateSampleData();

            XmlConfigurator.Configure(new FileInfo("log4net.config"));

            new DicomSetupBuilder()
               .RegisterServices(s => s.AddFellowOakDicom()
               .AddLogging(config =>
               {
                   // Казваме на Microsoft Logging да използва log4net
                   // По подразбиране ще търси файл "log4net.config"
                   config.AddLog4Net("log4net.config");
               }))
               .Build();

            var worklistServerPort = configuration.GetValue<int>("WorklistServer:Port", 8104); // Default port: 8104
            var worklistServerAET = configuration.GetValue<string>("WorklistServer:AET", "MODALITY_SCP"); // Default AET: MODALITY_SCP

            WorklistServer.Start(worklistServerPort, worklistServerAET); // Default DICOM port

            // PACS configuration
            checkPacs = configuration.GetValue<bool>("Pacs:CheckPacs", false); // Default: true
            pacsIp = configuration.GetValue<string>("Pacs:Ip", "127.0.0.1"); // Default: 127.0.0.1
            pacsPort = configuration.GetValue<int>("Pacs:Port", 9104); // Default: 9104
            pacsAET = configuration.GetValue<string>("Pacs:AET", "STOR"); // Default: STOR
            localAET = configuration.GetValue<string>("Pacs:LocalAET", "KOBOWORKLIST"); // Default: KOBOWORKLIST

            log.Debug($"PACS Configuration - CheckPacs: {checkPacs}, IP: {pacsIp}, Port: {pacsPort}, PACS AET: {pacsAET}, Local AET: {localAET}");

            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Start();
        }

        private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            log.Debug($"Стартиране на пакетна C-FIND проверка в: {e.SignalTime}");

            if (!checkPacs)
            {
                log.Debug("Пакетната C-FIND проверка е изключена в конфигурацията.");
                return;
            }

            try
            {
                // Use WorklistItemsProvider to get all current worklist items
                var worklistProvider = new WorklistItemsProvider(DatabasePath);
                var worklistItems = worklistProvider.GetAllCurrentWorklistItems();

                // Extract PatientID and AccessionNumber for C-FIND requests
                var patientsToSearch = new List<(string Id, string Acc)>();
                foreach (var item in worklistItems)
                {
                    if (!string.IsNullOrEmpty(item.PatientID) && !string.IsNullOrEmpty(item.AccessionNumber))
                    {
                        patientsToSearch.Add((item.PatientID, item.AccessionNumber));
                    }
                }

                var client = DicomClientFactory.Create(pacsIp, pacsPort, false, localAET, pacsAET);

                // Настройка на тайм-аут, за да не чакаме вечно, ако PACS-ът е зает
                client.NegotiateAsyncOps();

                foreach (var p in patientsToSearch)
                {
                    var request = DicomCFindRequest.CreateStudyQuery(patientId: p.Id, accession: p.Acc);

                    request.OnResponseReceived += (req, response) =>
                    {
                        if (response.Status == DicomStatus.Pending && response.HasDataset)
                        {
                            var name = response.Dataset.GetString(DicomTag.PatientName);
                            var uid = response.Dataset.GetString(DicomTag.StudyInstanceUID);
                            log.Info($"[FIND] Намерен: {name} (ID: {p.Id}, Acc: {p.Acc}) StudyUID: {uid}");
                        }
                        else if (response.Status == DicomStatus.Success)
                        {
                            log.Debug($"Заявката за ID {p.Id} ACCN {p.Acc} приключи успешно.");
                        }
                    };

                    await client.AddRequestAsync(request);
                }

                // Изпращаме всички заявки наведнъж
                await client.SendAsync();
                log.Debug("Пакетната C-FIND заявка приключи.");
            }
            catch (Exception ex)
            {
                log.Error("Критична грешка при пакетен C-FIND:", ex);
            }
        }

        private void PopulateSampleData()
        {
            using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={DatabasePath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO WorklistItems (AccessionNumber, DateOfBirth, PatientID, Surname, Forename, Sex, Modality, ExamDescription, ExamRoom, ProcedureID, ProcedureStepID, StudyUID, ScheduledAET, ReferringPhysician, ExamDateAndTime)
                VALUES ('AB123', '1975-02-14', '100015', 'Test', 'Hilbert', 'M', 'MR', 'mr knee left', 'MR1', '200001', '200002', '1.2.34.567890.1234567890.1', 'MRMODALITY', 'Smith^John^Md', '2026-01-15 10:00:00')";
            command.ExecuteNonQuery();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _timer?.Stop();
            _timer?.Dispose();
            WorklistServer.Stop();
            base.OnExit(e);
        }
    }
}
