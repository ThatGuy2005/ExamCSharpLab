# SmartLocker | Parcel terminal

A C# Windows Forms simulator for parcel intake, size-aware locker allocation, pickup, and compartment reuse. The project targets **.NET Framework 4.8** and uses a local SQLite database.

## Requirements

- Visual Studio with the .NET Framework 4.8 Developer Pack
- Restore the NuGet packages listed by the solution, including System.Data.SQLite

## Build and run

1. Open `ExamCSharpLab.slnx` in Visual Studio.
2. Restore NuGet packages if Visual Studio prompts you.
3. Select the `Debug | x64` configuration and build the solution.
4. Start the `ExamCSharpLab` application.

At startup, the application creates the database schema and default compartments if needed. It also ensures the database meets the dataset minimum. The **Generate dataset** button adds a new batch of 120 parcels each time it is clicked; it does not replace existing parcel rows.

## Database location

The repository creates `smartlocker.db` in the application's output directory, using `AppDomain.CurrentDomain.BaseDirectory`.

For the `Debug | x64` run configuration, the exact database file used is:

`C:\Users\Csongor\source\repos\ExamCSharpLab\ExamCSharpLab\bin\x64\Debug\smartlocker.db`

Submit this database file, and verify it contains at least 100 rows in the `Parcels` table. Running a different build configuration uses that configuration's output directory and therefore a different database file. Do not submit a different copy from the one used by the application.

## Using the application

- Enter a recipient and select a size and service, then choose **Add parcel**. New parcels are saved as Waiting.
- Choose **Generate dataset** to add 120 valid sample parcels to SQLite.
- Choose **Start processing** to allocate Waiting parcels to compatible free compartments. Express parcels are prioritized; boxes are assigned using best-fit size and then compartment ID.
- Choose **Stop processing** to request a cooperative stop after the active parcel finishes.
- Enter the six-digit pickup code and choose **Collect parcel**. Successful collection marks the parcel Collected and frees its compartment.
- The parcel grid, compartment overview, and counts are refreshed from the database.
- **Import XML** lets you select a SmartLocker XML file. Import replaces the existing parcel and compartment data after confirmation. The supplied sample has 120 compartments, so importing it changes the inventory from the exam's required 24 compartments to 120. For an exam run that must have exactly 24 compartments, do not import that 120-compartment sample.

## Data and implementation notes

- Generated dataset: 120 Waiting parcels, with 40 Small, 40 Medium, 40 Large, 60 Standard, and 60 Express parcels.
- The default locker unit contains 24 compartments: eight of each size.
- Parcel allocation and collection are persisted in SQLite transactions.
- Processing uses one background `System.Threading.Thread`; the form updates controls through UI-thread callbacks.
- The application uses LINQ for parcel selection and for grouping compartment occupancy by size.
- Storage and collection events are written with timestamps and parcel/compartment IDs to `trace.log`. Pickup codes are not written to the trace.

For `Debug | x64`, the trace file is generated at:

`C:\Users\Csongor\source\repos\ExamCSharpLab\ExamCSharpLab\bin\x64\Debug\trace.log`
