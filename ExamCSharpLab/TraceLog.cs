using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ExamCSharpLab
{
    /// <summary>Writes required parcel lifecycle events to a persistent trace.log file.</summary>
    public static class TraceLog
    {
        private static readonly object SyncRoot = new object();
        private static TextWriterTraceListener listener;

        /// <summary>Configures or replaces the file listener used by System.Diagnostics.Trace.</summary>
        public static void Initialize(string logPath = null)
        {
            lock (SyncRoot)
            {
                if (listener != null)
                {
                    Trace.Flush();
                    Trace.Listeners.Remove(listener);
                    listener.Dispose();
                }

                string path = Path.GetFullPath(logPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "trace.log"));
                string directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                listener = new TextWriterTraceListener(path);
                Trace.Listeners.Add(listener);
                Trace.AutoFlush = true;
            }
        }

        /// <summary>Records the parcel and compartment IDs after a successful storage operation.</summary>
        public static void ParcelStored(int parcelId, int compartmentId)
        {
            WriteEvent("Stored", parcelId, compartmentId);
        }

        /// <summary>Records the parcel and compartment IDs after a successful collection.</summary>
        public static void ParcelCollected(int parcelId, int compartmentId)
        {
            WriteEvent("Collected", parcelId, compartmentId);
        }

        private static void WriteEvent(string eventName, int parcelId, int compartmentId)
        {
            lock (SyncRoot)
            {
                // Initialize lazily if an event is emitted before the application entry point configures tracing.
                if (listener == null)
                {
                    Initialize();
                }

                Trace.WriteLine(string.Format("{0:O} {1} ParcelId={2} CompartmentId={3}",
                    DateTime.Now, eventName, parcelId, compartmentId));
                // Flush each event so it is available even if the application exits unexpectedly.
                Trace.Flush();
            }
        }
    }
}
