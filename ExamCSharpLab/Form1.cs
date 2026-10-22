using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ExamCSharpLab
{
    public partial class Form1 : Form
    {
        private readonly IParcelRepository parcelRepository;
        public Form1()
        {
            InitializeComponent();
            parcelRepository = new SqliteParcelRepository();
            parcelRepository.Initialize();
        }

        private void mockData_Click(object sender, EventArgs e)
        {
            string xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "smart_locker.xml");
            if (!File.Exists(xmlPath))
            {
                xmlPath = @"C:\\Users\\Csongor\\source\\repos\\ExamCSharpLab\\ExamCSharpLab\\bin\\x64\\Debug\\smart_locker.xml";
            }

            try
            {
                if (!File.Exists(xmlPath))
                {
                    throw new FileNotFoundException("Mock data file was not found.", xmlPath);
                }

                XDocument document = XDocument.Load(xmlPath);
                IEnumerable<XElement> elements = document.Root == null
                    ? Enumerable.Empty<XElement>()
                    : document.Root.DescendantsAndSelf();
                List<Parcel> parcels = elements
                    .Where(element => string.Equals(element.Name.LocalName, "Parcel", StringComparison.OrdinalIgnoreCase))
                    .Select(ParseParcel)
                    .ToList();

                if (parcels.Count == 0)
                {
                    throw new InvalidDataException("The XML file contains no Parcel elements.");
                }

                parcelRepository.AddParcels(parcels);
                MessageBox.Show(this, string.Format("Imported {0} parcels into {1}.", parcels.Count, parcelRepository.DatabasePath),
                    "Mock data loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Unable to load mock data", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static Parcel ParseParcel(XElement element)
        {
            string recipient = ReadField(element, "Recipient", "RecipientName", "Name");
            string sizeValue = ReadField(element, "Size");
            string serviceValue = ReadField(element, "ServiceType", "Service", "Type");

            if (string.IsNullOrWhiteSpace(recipient))
            {
                throw new InvalidDataException("Each Parcel must have a nonempty Recipient.");
            }

            ParcelSize size;
            switch ((sizeValue ?? string.Empty).Trim().ToUpperInvariant())
            {
                case "S":
                case "SMALL":
                    size = ParcelSize.Small;
                    break;
                case "M":
                case "MEDIUM":
                    size = ParcelSize.Medium;
                    break;
                case "L":
                case "LARGE":
                    size = ParcelSize.Large;
                    break;
                default:
                    throw new InvalidDataException("Size must be S/Small, M/Medium, or L/Large.");
            }

            string service = (serviceValue ?? string.Empty).Trim();
            Parcel parcel;
            if (string.Equals(service, "Express", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(service, "ExpressParcel", StringComparison.OrdinalIgnoreCase))
            {
                parcel = new ExpressParcel();
            }
            else if (string.Equals(service, "Standard", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(service, "StandardParcel", StringComparison.OrdinalIgnoreCase))
            {
                parcel = new StandardParcel();
            }
            else
            {
                throw new InvalidDataException("ServiceType must be Standard or Express.");
            }

            parcel.Recipient = recipient.Trim();
            parcel.Size = size;
            parcel.Status = ParcelStatus.Waiting;
            return parcel;
        }

        private static string ReadField(XElement element, params string[] names)
        {
            foreach (string name in names)
            {
                XAttribute attribute = element.Attributes().FirstOrDefault(item =>
                    string.Equals(item.Name.LocalName, name, StringComparison.OrdinalIgnoreCase));
                if (attribute != null)
                {
                    return attribute.Value;
                }

                XElement child = element.Elements().FirstOrDefault(item =>
                    string.Equals(item.Name.LocalName, name, StringComparison.OrdinalIgnoreCase));
                if (child != null)
                {
                    return child.Value;
                }
            }

            return null;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
