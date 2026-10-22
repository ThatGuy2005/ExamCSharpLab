using System.Collections.Generic;

namespace ExamCSharpLab
{
    /// <summary>Defines persistent parcel and compartment operations used by the application.</summary>
    public interface IParcelRepository
    {
        /// <summary>Full path to the SQLite database used by this repository.</summary>
        string DatabasePath { get; }

        /// <summary>Creates the database schema and its default compartments when needed.</summary>
        void Initialize();

        /// <summary>Inserts one new Waiting parcel and returns its generated ID.</summary>
        int AddParcel(Parcel parcel);

        /// <summary>Inserts a batch of new Waiting parcels.</summary>
        IList<Parcel> AddParcels(IEnumerable<Parcel> parcels);

        /// <summary>Atomically replaces all persisted rows with the supplied imported data.</summary>
        void ReplaceAllData(IEnumerable<LockerCompartment> compartments, IEnumerable<Parcel> parcels);

        /// <summary>Loads all parcels ordered by database ID.</summary>
        IList<Parcel> GetAllParcels();

        /// <summary>Loads all known physical compartments.</summary>
        IList<LockerCompartment> GetCompartments();

        /// <summary>Atomically assigns a compartment and pickup code to a Waiting parcel.</summary>
        bool TryStoreParcel(int parcelId, int compartmentId, string pickupCode);

        /// <summary>Marks the Stored parcel with this code Collected, or returns null when no match exists.</summary>
        Parcel CollectByPickupCode(string pickupCode);
    }
}
