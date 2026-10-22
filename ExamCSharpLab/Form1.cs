using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ExamCSharpLab
{
    /// <summary>Main WinForms interface for managing parcels, locker occupancy, imports, and processing.</summary>
    public partial class Form1 : Form
    {
        private readonly IParcelRepository parcelRepository;
        private readonly ParcelProcessingService processingService;

        /// <summary>Initializes persistence, the worker service, selectors, and the first database view.</summary>
        public Form1()
        {
            InitializeComponent();

            // Create the service after WinForms controls so it can capture the UI synchronization context.
            parcelRepository = new SqliteParcelRepository();
            parcelRepository.Initialize();
            processingService = new ParcelProcessingService(parcelRepository);

            sizeComboBox.Items.AddRange(new object[] { "S - Small", "M - Medium", "L - Large" });
            sizeComboBox.SelectedIndex = 0;
            serviceComboBox.Items.AddRange(new object[] { "Standard", "Express" });
            serviceComboBox.SelectedIndex = 0;

            addParcelButton.Click += addParcelButton_Click;
            generateDatasetButton.Click += generateDatasetButton_Click;
            importXmlButton.Click += importXmlButton_Click;
            startProcessingButton.Click += startProcessingButton_Click;
            stopProcessingButton.Click += stopProcessingButton_Click;
            collectParcelButton.Click += collectParcelButton_Click;
            FormClosing += Form1_FormClosing;

            RefreshData();
        }

        /// <summary>Reloads both grids and the status counts from persisted data.</summary>
        private void RefreshData()
        {
            IList<Parcel> parcels = parcelRepository.GetAllParcels();
            IList<LockerCompartment> compartments = parcelRepository.GetCompartments();
            // Only Stored parcels occupy boxes; collected parcels remain visible as history.
            IList<Parcel> storedParcels = parcels.Where(parcel => parcel.Status == ParcelStatus.Stored).ToList();

            // Project domain objects into simple row values that the WinForms grids can display directly.
            parcelGridView.DataSource = parcels.Select(parcel => new
            {
                parcel.Id,
                parcel.Recipient,
                Size = parcel.Size.ToString(),
                Service = parcel.ServiceType.ToString(),
                Status = parcel.Status.ToString(),
                parcel.CompartmentId,
                parcel.PickupCode
            }).ToList();

            // Join each physical box to its current Stored parcel to show free/occupied state.
            compartmentGridView.DataSource = compartments
                .OrderBy(compartment => compartment.Id)
                .Select(compartment =>
                {
                    Parcel occupant = storedParcels.FirstOrDefault(parcel => parcel.CompartmentId == compartment.Id);
                    return new
                    {
                        CompartmentId = compartment.Id,
                        Size = compartment.Size.ToString(),
                        Availability = occupant == null ? "Free" : "Occupied",
                        ParcelId = occupant == null ? (int?)null : occupant.Id,
                        Recipient = occupant == null ? string.Empty : occupant.Recipient
                    };
                })
                .ToList();

            // The service groups compartment occupancy by size using LINQ for the summary display.
            ParcelSummary summary = processingService.GetSummary();
            string sizeCounts = string.Join("    ", summary.CompartmentsBySize.Select(item =>
                string.Format("{0}: {1} free / {2} occupied", item.Size, item.FreeCount, item.OccupiedCount)));
            summaryLabel.Text = string.Format("Waiting: {0}    Stored: {1}    Collected: {2}    Free compartments: {3}    |    {4}",
                summary.WaitingCount, summary.StoredCount, summary.CollectedCount, summary.FreeCompartmentCount, sizeCounts);
        }

        /// <summary>Refreshes from a worker callback while ensuring all control access happens on the UI thread.</summary>
        private void RefreshDataSafely()
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action(RefreshData));
                return;
            }

            RefreshData();
        }

        /// <summary>Validates the entry fields and inserts one new Waiting parcel.</summary>
        private void addParcelButton_Click(object sender, EventArgs e)
        {
            string recipient = recipientTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(recipient))
            {
                MessageBox.Show(this, "Enter a recipient name.", "Invalid parcel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                recipientTextBox.Focus();
                return;
            }

            if (sizeComboBox.SelectedIndex < 0 || serviceComboBox.SelectedIndex < 0)
            {
                MessageBox.Show(this, "Select both a parcel size and service type.", "Invalid parcel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Parcel parcel = serviceComboBox.SelectedIndex == 1 ? (Parcel)new ExpressParcel() : new StandardParcel();
            parcel.Recipient = recipient;
            parcel.Size = (ParcelSize)(sizeComboBox.SelectedIndex + 1);
            parcel.Status = ParcelStatus.Waiting;

            try
            {
                int parcelId = parcelRepository.AddParcel(parcel);
                recipientTextBox.Clear();
                RefreshData();
                MessageBox.Show(this, string.Format("Parcel {0} was added and saved.", parcelId),
                    "Parcel added", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Unable to add parcel", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Generates and persists a balanced test dataset, then refreshes the displayed database state.</summary>
        private void generateDatasetButton_Click(object sender, EventArgs e)
        {
            try
            {
                IList<Parcel> generatedParcels = ParcelDatasetGenerator.GenerateDataset(parcelRepository);
                RefreshData();
                MessageBox.Show(this, string.Format("Generated and saved {0} parcels.{1}Database: {2}",
                    generatedParcels.Count, Environment.NewLine, parcelRepository.DatabasePath),
                    "Dataset generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Unable to generate dataset", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Loads XML locker and parcel records, confirms database replacement, and refreshes after commit.</summary>
        private void importXmlButton_Click(object sender, EventArgs e)
        {
            if (processingService.IsProcessing)
            {
                MessageBox.Show(this, "Stop processing and wait for it to finish before importing XML.",
                    "Processing is active", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select SmartLocker XML data";
                dialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    LockerImportData importData = ParseLockerXml(dialog.FileName);
                    string confirmation = string.Format(
                        "Replace all existing database data with {0} compartments and {1} parcels from this XML?{2}{2}{3}",
                        importData.Compartments.Count, importData.Parcels.Count, Environment.NewLine, dialog.FileName);
                    if (MessageBox.Show(this, confirmation, "Confirm XML import", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    {
                        return;
                    }

                    // Replacing is atomic in the repository, preserving XML IDs and active assignments together.
                    parcelRepository.ReplaceAllData(importData.Compartments, importData.Parcels);
                    RefreshData();
                    MessageBox.Show(this, string.Format("Imported {0} compartments and {1} parcels.{2}Database: {3}",
                        importData.Compartments.Count, importData.Parcels.Count, Environment.NewLine, parcelRepository.DatabasePath),
                        "XML import complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(this, exception.Message, "Unable to import XML", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>Reads the supported SmartLockerData structure before any database changes are made.</summary>
        private static LockerImportData ParseLockerXml(string path)
        {
            XDocument document = XDocument.Load(path);
            XElement root = document.Root;
            if (root == null || !string.Equals(root.Name.LocalName, "SmartLockerData", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("The XML root element must be SmartLockerData.");
            }

            // Compartments from each locker unit share one global ID space in the current database model.
            List<LockerCompartment> compartments = root.Elements()
                .Where(element => string.Equals(element.Name.LocalName, "LockerUnit", StringComparison.OrdinalIgnoreCase))
                .SelectMany(unit => unit.Elements()
                    .Where(element => string.Equals(element.Name.LocalName, "Compartment", StringComparison.OrdinalIgnoreCase)))
                .Select(ParseCompartment)
                .ToList();
            if (compartments.Count == 0)
            {
                throw new InvalidDataException("The XML file contains no LockerUnit compartments.");
            }

            XElement parcelContainer = root.Elements()
                .FirstOrDefault(element => string.Equals(element.Name.LocalName, "Parcels", StringComparison.OrdinalIgnoreCase));
            if (parcelContainer == null)
            {
                throw new InvalidDataException("The XML file does not contain a Parcels element.");
            }

            List<Parcel> parcels = parcelContainer.Elements()
                .Where(element => string.Equals(element.Name.LocalName, "Parcel", StringComparison.OrdinalIgnoreCase))
                .Select(ParseImportedParcel)
                .ToList();
            if (parcels.Count == 0)
            {
                throw new InvalidDataException("The Parcels element contains no Parcel child elements.");
            }

            return new LockerImportData { Compartments = compartments, Parcels = parcels };
        }

        /// <summary>Parses one compartment's required ID and size attributes.</summary>
        private static LockerCompartment ParseCompartment(XElement element)
        {
            int id = ParseRequiredIntAttribute(element, "Id");
            ParcelSize size = ParseSize(ReadAttribute(element, "Size"));
            return new LockerCompartment(id, size);
        }

        /// <summary>Parses parcel identity and state, translating the XML's InLocker state to Stored.</summary>
        private static Parcel ParseImportedParcel(XElement element)
        {
            int id = ParseRequiredIntAttribute(element, "Id");
            string recipient = ReadAttribute(element, "Recipient");
            if (string.IsNullOrWhiteSpace(recipient))
            {
                throw new InvalidDataException(string.Format("Parcel {0} must have a Recipient.", id));
            }

            string serviceType = ReadAttribute(element, "ServiceType");
            Parcel parcel;
            if (string.Equals(serviceType, "Express", StringComparison.OrdinalIgnoreCase))
            {
                parcel = new ExpressParcel();
            }
            else if (string.Equals(serviceType, "Standard", StringComparison.OrdinalIgnoreCase))
            {
                parcel = new StandardParcel();
            }
            else
            {
                throw new InvalidDataException(string.Format("Parcel {0} has an invalid ServiceType.", id));
            }

            string statusValue = ReadAttribute(element, "Status");
            ParcelStatus status;
            if (string.Equals(statusValue, "Waiting", StringComparison.OrdinalIgnoreCase))
            {
                status = ParcelStatus.Waiting;
            }
            else if (string.Equals(statusValue, "Stored", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(statusValue, "InLocker", StringComparison.OrdinalIgnoreCase))
            {
                // InLocker is the XML vocabulary for the application's persisted Stored state.
                status = ParcelStatus.Stored;
            }
            else if (string.Equals(statusValue, "Collected", StringComparison.OrdinalIgnoreCase))
            {
                status = ParcelStatus.Collected;
            }
            else
            {
                throw new InvalidDataException(string.Format("Parcel {0} has an invalid Status.", id));
            }

            string compartmentValue = ReadAttribute(element, "CompartmentId");
            int? compartmentId = string.IsNullOrWhiteSpace(compartmentValue)
                ? (int?)null
                : ParseInteger(compartmentValue, "CompartmentId", id);

            parcel.Id = id;
            parcel.Recipient = recipient.Trim();
            parcel.Size = ParseSize(ReadAttribute(element, "Size"));
            parcel.Status = status;
            parcel.CompartmentId = compartmentId;
            parcel.PickupCode = ReadAttribute(element, "PickupCode");
            return parcel;
        }

        /// <summary>Parses the supported letter or full-name representation of a parcel size.</summary>
        private static ParcelSize ParseSize(string value)
        {
            switch ((value ?? string.Empty).Trim().ToUpperInvariant())
            {
                case "S":
                case "SMALL":
                    return ParcelSize.Small;
                case "M":
                case "MEDIUM":
                    return ParcelSize.Medium;
                case "L":
                case "LARGE":
                    return ParcelSize.Large;
                default:
                    throw new InvalidDataException("Size must be Small, Medium, or Large.");
            }
        }

        /// <summary>Reads a required positive integer attribute, such as an imported record ID.</summary>
        private static int ParseRequiredIntAttribute(XElement element, string name)
        {
            string value = ReadAttribute(element, name);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidDataException(string.Format("{0} element must have an {1} attribute.", element.Name.LocalName, name));
            }

            return ParseInteger(value, name, null);
        }

        /// <summary>Uses invariant parsing so imported numeric IDs do not depend on the computer's locale.</summary>
        private static int ParseInteger(string value, string attributeName, int? parcelId)
        {
            int result;
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result) || result <= 0)
            {
                string owner = parcelId.HasValue ? string.Format(" on parcel {0}", parcelId.Value) : string.Empty;
                throw new InvalidDataException(string.Format("{0}{1} must be a positive integer.", attributeName, owner));
            }

            return result;
        }

        /// <summary>Reads an XML attribute by local name without case sensitivity.</summary>
        private static string ReadAttribute(XElement element, string name)
        {
            XAttribute attribute = element.Attributes().FirstOrDefault(item =>
                string.Equals(item.Name.LocalName, name, StringComparison.OrdinalIgnoreCase));
            return attribute == null ? null : attribute.Value;
        }

        /// <summary>Holds the fully parsed XML data until the user confirms its database replacement.</summary>
        private sealed class LockerImportData
        {
            public IList<LockerCompartment> Compartments { get; set; }
            public IList<Parcel> Parcels { get; set; }
        }

        /// <summary>Starts the single background processing worker and updates the action-button states.</summary>
        private void startProcessingButton_Click(object sender, EventArgs e)
        {
            processingService.StartProcessing(
                onUpdated: RefreshDataSafely,
                onError: HandleProcessingError,
                onFinished: HandleProcessingFinished);

            startProcessingButton.Enabled = false;
            stopProcessingButton.Enabled = true;
        }

        /// <summary>Requests a cooperative worker stop after its current parcel finishes.</summary>
        private void stopProcessingButton_Click(object sender, EventArgs e)
        {
            processingService.StopProcessing();
            stopProcessingButton.Enabled = false;
        }

        /// <summary>Validates a six-digit pickup code and collects the matching Stored parcel.</summary>
        private void collectParcelButton_Click(object sender, EventArgs e)
        {
            string pickupCode = pickupCodeTextBox.Text.Trim();
            if (pickupCode.Length != 6 || pickupCode.Any(character => !char.IsDigit(character)))
            {
                MessageBox.Show(this, "Enter the six-digit pickup code.", "Invalid pickup code", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                pickupCodeTextBox.Focus();
                return;
            }

            try
            {
                Parcel parcel = processingService.CollectParcel(pickupCode);
                if (parcel == null)
                {
                    MessageBox.Show(this, "No stored parcel was found for that pickup code.",
                        "Parcel not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                pickupCodeTextBox.Clear();
                RefreshData();
                MessageBox.Show(this, string.Format("Parcel {0} was collected. Compartment {1} is free.",
                    parcel.Id, parcel.CompartmentId), "Parcel collected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Unable to collect parcel", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Displays worker failures on the UI thread.</summary>
        private void HandleProcessingError(Exception exception)
        {
            RunOnUiThread(() => MessageBox.Show(this, exception.Message, "Processing error",
                MessageBoxButtons.OK, MessageBoxIcon.Error));
        }

        /// <summary>Restores the processing buttons and refreshes the view when the worker exits.</summary>
        private void HandleProcessingFinished()
        {
            RunOnUiThread(() =>
            {
                startProcessingButton.Enabled = true;
                stopProcessingButton.Enabled = false;
                RefreshData();
            });
        }

        /// <summary>Runs a callback directly on the UI thread or posts it there when called by the worker.</summary>
        private void RunOnUiThread(Action action)
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(action);
            }
            else
            {
                action();
            }
        }

        /// <summary>Requests cooperative shutdown when the form closes.</summary>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            processingService.StopProcessing();
        }
    }
}