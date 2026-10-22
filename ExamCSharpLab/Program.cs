using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExamCSharpLab
{
    /// <summary>Configures and starts the Windows Forms application.</summary>
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Set up persistent trace output and ensure the database has its schema and minimum dataset before showing the UI.
            TraceLog.Initialize();
            var repository = new SqliteParcelRepository();
            repository.Initialize();
            ParcelDatasetGenerator.EnsureMinimumDataset(repository);

            Application.Run(new Form1());
        }
    }
}
