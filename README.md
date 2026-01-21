# KoboWorklist

KoboWorklist is a **DICOM Modality Worklist Server** designed to manage and provide worklist items to DICOM-compatible modalities. It is built using the **Fellow Oak DICOM (fo-dicom)** library and supports integration with PACS (Picture Archiving and Communication System) for querying and managing patient data.

---

## Features

- **DICOM Modality Worklist (MWL) Server**:
  - Provides worklist items to DICOM modalities such as CT, MRI, and Ultrasound devices.
  - Configurable DICOM Application Entity Title (AET) and port.

- **Database-Driven Worklist Management**:
  - Stores worklist items in a SQLite database.
  - Supports adding, updating, and deleting worklist items.

- **PACS Integration**:
  - Queries PACS servers using DICOM C-FIND requests.
  - Retrieves patient and study information from PACS.

- **Configurable Settings**:
  - All critical settings, such as server port, AET, PACS IP, and PACS port, are configurable via `appsettings.json`.

- **Logging**:
  - Uses `log4net` for detailed logging of server operations and PACS queries.

- **WPF User Interface**:
  - Provides a graphical interface for managing worklist items.
  - Allows editing patient and study details.

---

## How It Works

1. **DICOM Worklist Server**:
   - The application starts a DICOM Worklist Server using the `WorklistServer.Start` method.
   - The server listens for incoming DICOM C-FIND requests from modalities and responds with matching worklist items.

2. **Worklist Management**:
   - Worklist items are stored in a SQLite database (`WorklistItems.db`).
   - The `WorklistItemsProvider` class handles database operations, such as retrieving, adding, updating, and deleting worklist items.

3. **PACS Query**:
   - The application periodically queries a PACS server for patient and study data using DICOM C-FIND requests.
   - The PACS configuration (IP, port, AET) is loaded from `appsettings.json`.

4. **WPF Interface**:
   - Users can manage worklist items through the WPF interface.
   - The `EditWorklistItemWindow` allows users to edit patient and study details.

---

## Configuration

The application uses an `appsettings.json` file for configuration. Below is an example configuration:

```json
{
  "WorklistServer": {
    "Port": 8080,
    "AET": "KoboWorklist"
  },
  "Pacs": {
    "CheckPacs": true,
    "Ip": "127.0.0.1",
    "Port": 105,
    "AET": "PACS_AET",
    "LocalAET": "KoboWorklist"
  },
  "Modalities": [ "CT", "MR", "US" ]
}
```

### Key Settings:
- **WorklistServer**:
  - `Port`: The port on which the DICOM Worklist Server listens.
  - `AET`: The Application Entity Title of the Worklist Server.

- **Pacs**:
  - `CheckPacs`: Enables or disables PACS querying.
  - `Ip`: The IP address of the PACS server.
  - `Port`: The port of the PACS server.
  - `AET`: The Application Entity Title of the PACS server.
  - `LocalAET`: The Application Entity Title of the Worklist Server when communicating with PACS.

- **Modalities**:
  - A list of supported modalities (e.g., CT, MR, US).

---

## Database

The SQLite database (`WorklistItems.db`) contains the following fields for each worklist item:
- `PatientID`
- `AccessionNumber`
- `Surname`
- `Forename`
- `DateOfBirth`
- `Sex`
- `Modality`
- `ExamDescription`
- `StudyUID`
- `ScheduledAET`
- `ReferringPhysician`
- `ExamDateAndTime`

---

## How to Run

1. **Build and Run**:
   - Open the solution in Visual Studio.
   - Build and run the application.

2. **Configure Settings**:
   - Update `appsettings.json` with the desired configuration.

3. **Manage Worklist Items**:
   - Use the WPF interface to add, edit, or delete worklist items.

4. **PACS Query**:
   - Ensure the PACS server is reachable and configured correctly in `appsettings.json`.

---

## Dependencies

- **Fellow Oak DICOM (fo-dicom)**: For DICOM communication.
- **SQLite**: For database storage.
- **log4net**: For logging.
- **Microsoft.Extensions.Configuration**: For configuration management.

---

## License

This project is licensed under the **Microsoft Public License (MS-PL)**.

---

## Contributing

Contributions are welcome! Please follow the guidelines in `CONTRIBUTING.md` when submitting pull requests.

---

## Contact

For questions or support, please contact the repository owner or create an issue in the GitHub repository.
