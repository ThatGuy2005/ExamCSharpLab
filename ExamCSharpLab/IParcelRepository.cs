using System.Collections.Generic;

namespace ExamCSharpLab
{
    public interface IParcelRepository
    {
        string DatabasePath { get; }
        void Initialize();
        int AddParcel(Parcel parcel);
        IList<Parcel> AddParcels(IEnumerable<Parcel> parcels);
        IList<Parcel> GetAllParcels();
        IList<LockerCompartment> GetCompartments();
        bool TryStoreParcel(int parcelId, int compartmentId, string pickupCode);
        Parcel CollectByPickupCode(string pickupCode);
    }
}
