using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace ExamCSharpLab
{
    public sealed class CompartmentSizeCount
    {
        public ParcelSize Size { get; set; }
        public int FreeCount { get; set; }
        public int OccupiedCount { get; set; }
    }

    public sealed class ParcelSummary
    {
        public int WaitingCount { get; set; }
        public int StoredCount { get; set; }
        public int CollectedCount { get; set; }
        public IList<CompartmentSizeCount> CompartmentsBySize { get; set; }

        public int FreeCompartmentCount
        {
            get { return CompartmentsBySize == null ? 0 : CompartmentsBySize.Sum(c => c.FreeCount); }
        }
    }

    public sealed class ParcelProcessingService
    {
        private readonly IParcelRepository repository;
        private readonly SynchronizationContext synchronizationContext;
        private readonly object threadLock = new object();
        private Thread processingThread;
        private volatile bool stopRequested;

        public ParcelProcessingService(IParcelRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException("repository");
            synchronizationContext = SynchronizationContext.Current;
        }

        public bool IsProcessing
        {
            get
            {
                lock (threadLock)
                {
                    return processingThread != null && processingThread.IsAlive;
                }
            }
        }

        public void StartProcessing(Action onUpdated = null, Action<Exception> onError = null, Action onFinished = null)
        {
            lock (threadLock)
            {
                if (processingThread != null && processingThread.IsAlive)
                {
                    return;
                }

                stopRequested = false;
                processingThread = new Thread(() => ProcessQueue(onUpdated, onError, onFinished));
                processingThread.IsBackground = true;
                processingThread.Name = "Parcel processing";
                processingThread.Start();
            }
        }

        public void StopProcessing()
        {
            stopRequested = true;
        }

        public Parcel CollectParcel(string pickupCode)
        {
            Parcel parcel = repository.CollectByPickupCode(pickupCode);
            if (parcel != null && parcel.CompartmentId.HasValue)
            {
                TraceLog.ParcelCollected(parcel.Id, parcel.CompartmentId.Value);
            }

            return parcel;
        }

        public ParcelSummary GetSummary()
        {
            IList<Parcel> parcels = repository.GetAllParcels();
            IList<LockerCompartment> compartments = repository.GetCompartments();
            var storedCompartmentIds = new HashSet<int>(parcels
                .Where(p => p.Status == ParcelStatus.Stored && p.CompartmentId.HasValue)
                .Select(p => p.CompartmentId.Value));

            var occupancy = compartments.Select(compartment => new
            {
                Compartment = compartment,
                IsOccupied = storedCompartmentIds.Contains(compartment.Id)
            });
            IList<CompartmentSizeCount> groupedCounts = occupancy
                .GroupBy(item => item.Compartment.Size)
                .OrderBy(group => group.Key)
                .Select(group => new CompartmentSizeCount
                {
                    Size = group.Key,
                    FreeCount = group.Count(item => !item.IsOccupied),
                    OccupiedCount = group.Count(item => item.IsOccupied)
                })
                .ToList();

            return new ParcelSummary
            {
                WaitingCount = parcels.Count(p => p.Status == ParcelStatus.Waiting),
                StoredCount = parcels.Count(p => p.Status == ParcelStatus.Stored),
                CollectedCount = parcels.Count(p => p.Status == ParcelStatus.Collected),
                CompartmentsBySize = groupedCounts
            };
        }

        private void ProcessQueue(Action onUpdated, Action<Exception> onError, Action onFinished)
        {
            try
            {
                while (!stopRequested)
                {
                    IList<Parcel> parcels = repository.GetAllParcels();
                    IList<LockerCompartment> compartments = repository.GetCompartments();
                    var occupiedIds = new HashSet<int>(parcels
                        .Where(p => p.Status == ParcelStatus.Stored && p.CompartmentId.HasValue)
                        .Select(p => p.CompartmentId.Value));
                    var freeCompartments = compartments.Where(c => !occupiedIds.Contains(c.Id)).ToList();

                    Parcel candidate = parcels
                        .Where(p => p.Status == ParcelStatus.Waiting)
                        .OrderBy(p => p.ServiceType == ParcelServiceType.Express ? 0 : 1)
                        .ThenBy(p => p.Id)
                        .FirstOrDefault(p => freeCompartments.Any(c => (int)c.Size >= (int)p.Size));

                    if (candidate == null)
                    {
                        break;
                    }

                    LockerCompartment target = freeCompartments
                        .Where(c => (int)c.Size >= (int)candidate.Size)
                        .OrderBy(c => (int)c.Size)
                        .ThenBy(c => c.Id)
                        .First();

                    Thread.Sleep(candidate.GetProcessingDelayMs());
                    bool stored = false;
                    for (int attempt = 0; attempt < 20 && !stored; attempt++)
                    {
                        stored = repository.TryStoreParcel(candidate.Id, target.Id, CreatePickupCode());
                    }

                    if (stored)
                    {
                        TraceLog.ParcelStored(candidate.Id, target.Id);
                        PostToCapturedContext(onUpdated);
                    }
                }
            }
            catch (Exception exception)
            {
                PostToCapturedContext(() => { if (onError != null) onError(exception); });
            }
            finally
            {
                lock (threadLock)
                {
                    processingThread = null;
                }

                PostToCapturedContext(onFinished);
            }
        }

        private void PostToCapturedContext(Action callback)
        {
            if (callback == null)
            {
                return;
            }

            if (synchronizationContext == null)
            {
                callback();
                return;
            }

            synchronizationContext.Post(state => callback(), null);
        }

        private static string CreatePickupCode()
        {
            byte[] bytes = new byte[4];
            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(bytes);
            }

            uint value = BitConverter.ToUInt32(bytes, 0) % 900000;
            return (value + 100000).ToString("D6");
        }
    }
}
