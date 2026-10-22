using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ExamCSharpLab
{
    public static class TraceLog
    {
        private static readonly object SyncRoot = new object();
        private static TextWriterTraceListener listener;

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

        public static void ParcelStored(int parcelId, int compartmentId)
        {
            WriteEvent("Stored", parcelId, compartmentId);
        }

        public static void ParcelCollected(int parcelId, int compartmentId)
        {
            WriteEvent("Collected", parcelId, compartmentId);
        }

        private static void WriteEvent(string eventName, int parcelId, int compartmentId)
        {
            lock (SyncRoot)
            {
                if (listener == null)
                {
                    Initialize();
                }

                Trace.WriteLine(string.Format("{0:O} {1} ParcelId={2} CompartmentId={3}",
                    DateTime.Now, eventName, parcelId, compartmentId));
                Trace.Flush();
            }
        }
    }
}
