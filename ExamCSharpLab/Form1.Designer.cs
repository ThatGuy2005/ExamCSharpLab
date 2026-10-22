namespace ExamCSharpLab
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel inputPanel;
        private System.Windows.Forms.Label recipientLabel;
        private System.Windows.Forms.TextBox recipientTextBox;
        private System.Windows.Forms.Label sizeLabel;
        private System.Windows.Forms.ComboBox sizeComboBox;
        private System.Windows.Forms.Label serviceLabel;
        private System.Windows.Forms.ComboBox serviceComboBox;
        private System.Windows.Forms.Button addParcelButton;
        private System.Windows.Forms.Button generateDatasetButton;
        private System.Windows.Forms.Label pickupCodeLabel;
        private System.Windows.Forms.TextBox pickupCodeTextBox;
        private System.Windows.Forms.Button collectParcelButton;
        private System.Windows.Forms.Button startProcessingButton;
        private System.Windows.Forms.Button stopProcessingButton;
        private System.Windows.Forms.Label summaryLabel;
        private System.Windows.Forms.SplitContainer dataSplitContainer;
        private System.Windows.Forms.DataGridView parcelGridView;
        private System.Windows.Forms.DataGridView compartmentGridView;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.inputPanel = new System.Windows.Forms.Panel();
            this.recipientLabel = new System.Windows.Forms.Label();
            this.recipientTextBox = new System.Windows.Forms.TextBox();
            this.sizeLabel = new System.Windows.Forms.Label();
            this.sizeComboBox = new System.Windows.Forms.ComboBox();
            this.serviceLabel = new System.Windows.Forms.Label();
            this.serviceComboBox = new System.Windows.Forms.ComboBox();
            this.addParcelButton = new System.Windows.Forms.Button();
            this.generateDatasetButton = new System.Windows.Forms.Button();
            this.pickupCodeLabel = new System.Windows.Forms.Label();
            this.pickupCodeTextBox = new System.Windows.Forms.TextBox();
            this.collectParcelButton = new System.Windows.Forms.Button();
            this.startProcessingButton = new System.Windows.Forms.Button();
            this.stopProcessingButton = new System.Windows.Forms.Button();
            this.summaryLabel = new System.Windows.Forms.Label();
            this.dataSplitContainer = new System.Windows.Forms.SplitContainer();
            this.parcelGridView = new System.Windows.Forms.DataGridView();
            this.compartmentGridView = new System.Windows.Forms.DataGridView();
            this.inputPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataSplitContainer)).BeginInit();
            this.dataSplitContainer.Panel1.SuspendLayout();
            this.dataSplitContainer.Panel2.SuspendLayout();
            this.dataSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.parcelGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.compartmentGridView)).BeginInit();
            this.SuspendLayout();
            //
            // inputPanel
            //
            this.inputPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.inputPanel.Controls.Add(this.recipientLabel);
            this.inputPanel.Controls.Add(this.recipientTextBox);
            this.inputPanel.Controls.Add(this.sizeLabel);
            this.inputPanel.Controls.Add(this.sizeComboBox);
            this.inputPanel.Controls.Add(this.serviceLabel);
            this.inputPanel.Controls.Add(this.serviceComboBox);
            this.inputPanel.Controls.Add(this.addParcelButton);
            this.inputPanel.Controls.Add(this.generateDatasetButton);
            this.inputPanel.Controls.Add(this.pickupCodeLabel);
            this.inputPanel.Controls.Add(this.pickupCodeTextBox);
            this.inputPanel.Controls.Add(this.collectParcelButton);
            this.inputPanel.Controls.Add(this.startProcessingButton);
            this.inputPanel.Controls.Add(this.stopProcessingButton);
            this.inputPanel.Location = new System.Drawing.Point(12, 12);
            this.inputPanel.Name = "inputPanel";
            this.inputPanel.Size = new System.Drawing.Size(1256, 124);
            this.inputPanel.TabIndex = 0;
            //
            // recipientLabel
            //
            this.recipientLabel.AutoSize = true;
            this.recipientLabel.Location = new System.Drawing.Point(8, 10);
            this.recipientLabel.Name = "recipientLabel";
            this.recipientLabel.Size = new System.Drawing.Size(61, 15);
            this.recipientLabel.TabIndex = 0;
            this.recipientLabel.Text = "Recipient";
            //
            // recipientTextBox
            //
            this.recipientTextBox.Location = new System.Drawing.Point(8, 31);
            this.recipientTextBox.Name = "recipientTextBox";
            this.recipientTextBox.Size = new System.Drawing.Size(250, 23);
            this.recipientTextBox.TabIndex = 1;
            //
            // sizeLabel
            //
            this.sizeLabel.AutoSize = true;
            this.sizeLabel.Location = new System.Drawing.Point(274, 10);
            this.sizeLabel.Name = "sizeLabel";
            this.sizeLabel.Size = new System.Drawing.Size(27, 15);
            this.sizeLabel.TabIndex = 2;
            this.sizeLabel.Text = "Size";
            //
            // sizeComboBox
            //
            this.sizeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sizeComboBox.FormattingEnabled = true;
            this.sizeComboBox.Location = new System.Drawing.Point(274, 31);
            this.sizeComboBox.Name = "sizeComboBox";
            this.sizeComboBox.Size = new System.Drawing.Size(120, 23);
            this.sizeComboBox.TabIndex = 3;
            //
            // serviceLabel
            //
            this.serviceLabel.AutoSize = true;
            this.serviceLabel.Location = new System.Drawing.Point(411, 10);
            this.serviceLabel.Name = "serviceLabel";
            this.serviceLabel.Size = new System.Drawing.Size(44, 15);
            this.serviceLabel.TabIndex = 4;
            this.serviceLabel.Text = "Service";
            //
            // serviceComboBox
            //
            this.serviceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.serviceComboBox.FormattingEnabled = true;
            this.serviceComboBox.Location = new System.Drawing.Point(411, 31);
            this.serviceComboBox.Name = "serviceComboBox";
            this.serviceComboBox.Size = new System.Drawing.Size(130, 23);
            this.serviceComboBox.TabIndex = 5;
            //
            // addParcelButton
            //
            this.addParcelButton.Location = new System.Drawing.Point(558, 29);
            this.addParcelButton.Name = "addParcelButton";
            this.addParcelButton.Size = new System.Drawing.Size(110, 27);
            this.addParcelButton.TabIndex = 6;
            this.addParcelButton.Text = "Add parcel";
            this.addParcelButton.UseVisualStyleBackColor = true;
            //
            // generateDatasetButton
            //
            this.generateDatasetButton.Location = new System.Drawing.Point(681, 29);
            this.generateDatasetButton.Name = "generateDatasetButton";
            this.generateDatasetButton.Size = new System.Drawing.Size(135, 27);
            this.generateDatasetButton.TabIndex = 7;
            this.generateDatasetButton.Text = "Generate dataset";
            this.generateDatasetButton.UseVisualStyleBackColor = true;
            //
            // pickupCodeLabel
            //
            this.pickupCodeLabel.AutoSize = true;
            this.pickupCodeLabel.Location = new System.Drawing.Point(8, 74);
            this.pickupCodeLabel.Name = "pickupCodeLabel";
            this.pickupCodeLabel.Size = new System.Drawing.Size(71, 15);
            this.pickupCodeLabel.TabIndex = 8;
            this.pickupCodeLabel.Text = "Pickup code";
            //
            // pickupCodeTextBox
            //
            this.pickupCodeTextBox.Location = new System.Drawing.Point(8, 94);
            this.pickupCodeTextBox.MaxLength = 6;
            this.pickupCodeTextBox.Name = "pickupCodeTextBox";
            this.pickupCodeTextBox.Size = new System.Drawing.Size(140, 23);
            this.pickupCodeTextBox.TabIndex = 9;
            //
            // collectParcelButton
            //
            this.collectParcelButton.Location = new System.Drawing.Point(162, 92);
            this.collectParcelButton.Name = "collectParcelButton";
            this.collectParcelButton.Size = new System.Drawing.Size(115, 27);
            this.collectParcelButton.TabIndex = 10;
            this.collectParcelButton.Text = "Collect parcel";
            this.collectParcelButton.UseVisualStyleBackColor = true;
            //
            // startProcessingButton
            //
            this.startProcessingButton.Location = new System.Drawing.Point(309, 92);
            this.startProcessingButton.Name = "startProcessingButton";
            this.startProcessingButton.Size = new System.Drawing.Size(130, 27);
            this.startProcessingButton.TabIndex = 11;
            this.startProcessingButton.Text = "Start processing";
            this.startProcessingButton.UseVisualStyleBackColor = true;
            //
            // stopProcessingButton
            //
            this.stopProcessingButton.Enabled = false;
            this.stopProcessingButton.Location = new System.Drawing.Point(451, 92);
            this.stopProcessingButton.Name = "stopProcessingButton";
            this.stopProcessingButton.Size = new System.Drawing.Size(125, 27);
            this.stopProcessingButton.TabIndex = 12;
            this.stopProcessingButton.Text = "Stop processing";
            this.stopProcessingButton.UseVisualStyleBackColor = true;
            //
            // summaryLabel
            //
            this.summaryLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.summaryLabel.Location = new System.Drawing.Point(12, 145);
            this.summaryLabel.Name = "summaryLabel";
            this.summaryLabel.Size = new System.Drawing.Size(1256, 28);
            this.summaryLabel.TabIndex = 1;
            this.summaryLabel.Text = "Waiting: 0    Stored: 0    Collected: 0    Free compartments: 0";
            this.summaryLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // dataSplitContainer
            //
            this.dataSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.dataSplitContainer.Location = new System.Drawing.Point(12, 180);
            this.dataSplitContainer.Name = "dataSplitContainer";
            this.dataSplitContainer.Panel1.Controls.Add(this.parcelGridView);
            this.dataSplitContainer.Panel2.Controls.Add(this.compartmentGridView);
            this.dataSplitContainer.Size = new System.Drawing.Size(1256, 588);
            this.dataSplitContainer.SplitterDistance = 875;
            this.dataSplitContainer.TabIndex = 2;
            //
            // parcelGridView
            //
            this.parcelGridView.AllowUserToAddRows = false;
            this.parcelGridView.AllowUserToDeleteRows = false;
            this.parcelGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.parcelGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.parcelGridView.ReadOnly = true;
            this.parcelGridView.RowHeadersVisible = false;
            this.parcelGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.parcelGridView.Name = "parcelGridView";
            this.parcelGridView.TabIndex = 0;
            //
            // compartmentGridView
            //
            this.compartmentGridView.AllowUserToAddRows = false;
            this.compartmentGridView.AllowUserToDeleteRows = false;
            this.compartmentGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.compartmentGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.compartmentGridView.ReadOnly = true;
            this.compartmentGridView.RowHeadersVisible = false;
            this.compartmentGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.compartmentGridView.Name = "compartmentGridView";
            this.compartmentGridView.TabIndex = 0;
            //
            // Form1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 780);
            this.Controls.Add(this.dataSplitContainer);
            this.Controls.Add(this.summaryLabel);
            this.Controls.Add(this.inputPanel);
            this.MinimumSize = new System.Drawing.Size(1000, 600);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SmartLocker | Parcel terminal";
            this.inputPanel.ResumeLayout(false);
            this.inputPanel.PerformLayout();
            this.dataSplitContainer.Panel1.ResumeLayout(false);
            this.dataSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataSplitContainer)).EndInit();
            this.dataSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.parcelGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.compartmentGridView)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
