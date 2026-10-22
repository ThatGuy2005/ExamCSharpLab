using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ExamCSharpLab
{
    public partial class Form1 : Form
    {
        private readonly IParcelRepository parcelRepository;
        private readonly ParcelProcessingService processingService;

        public Form1()
        {
            InitializeComponent();

            parcelRepository = new SqliteParcelRepository();
            parcelRepository.Initialize();
            processingService = new ParcelProcessingService(parcelRepository);

            sizeComboBox.Items.AddRange(new object[] { "S - Small", "M - Medium", "L - Large" });
            sizeComboBox.SelectedIndex = 0;
            serviceComboBox.Items.AddRange(new object[] { "Standard", "Express" });
            serviceComboBox.SelectedIndex = 0;

            addParcelButton.Click += addParcelButton_Click;
            generateDatasetButton.Click += generateDatasetButton_Click;
            startProcessingButton.Click += startProcessingButton_Click;
            stopProcessingButton.Click += stopProcessingButton_Click;
            collectParcelButton.Click += collectParcelButton_Click;
            FormClosing += Form1_FormClosing;

            RefreshData();
        }

        private void RefreshData()
        {
            IList<Parcel> parcels = parcelRepository.GetAllParcels();
            IList<LockerCompartment> compartments = parcelRepository.GetCompartments();
            IList<Parcel> storedParcels = parcels.Where(parcel => parcel.Status == ParcelStatus.Stored).ToList();

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

            ParcelSummary summary = processingService.GetSummary();
            string sizeCounts = string.Join("    ", summary.CompartmentsBySize.Select(item =>
                string.Format("{0}: {1} free / {2} occupied", item.Size, item.FreeCount, item.OccupiedCount)));
            summaryLabel.Text = string.Format("Waiting: {0}    Stored: {1}    Collected: {2}    Free compartments: {3}    |    {4}",
                summary.WaitingCount, summary.StoredCount, summary.CollectedCount, summary.FreeCompartmentCount, sizeCounts);
        }

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

        private void startProcessingButton_Click(object sender, EventArgs e)
        {
            processingService.StartProcessing(
                onUpdated: RefreshDataSafely,
                onError: HandleProcessingError,
                onFinished: HandleProcessingFinished);

            startProcessingButton.Enabled = false;
            stopProcessingButton.Enabled = true;
        }

        private void stopProcessingButton_Click(object sender, EventArgs e)
        {
            processingService.StopProcessing();
            stopProcessingButton.Enabled = false;
        }

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

        private void HandleProcessingError(Exception exception)
        {
            RunOnUiThread(() => MessageBox.Show(this, exception.Message, "Processing error",
                MessageBoxButtons.OK, MessageBoxIcon.Error));
        }

        private void HandleProcessingFinished()
        {
            RunOnUiThread(() =>
            {
                startProcessingButton.Enabled = true;
                stopProcessingButton.Enabled = false;
                RefreshData();
            });
        }

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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            processingService.StopProcessing();
        }
    }
}