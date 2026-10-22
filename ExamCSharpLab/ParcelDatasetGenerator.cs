using System;
using System.Collections.Generic;
using System.Linq;

namespace ExamCSharpLab
{
    public static class ParcelDatasetGenerator
    {
        public static IList<Parcel> GenerateDataset(IParcelRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            var parcels = new List<Parcel>();
            for (int index = 1; index <= 120; index++)
            {
                Parcel parcel = index % 2 == 0 ? (Parcel)new ExpressParcel() : new StandardParcel();
                parcel.Recipient = "Generated Recipient " + index.ToString("D3");
                parcel.Size = (ParcelSize)(((index - 1) % 3) + 1);
                parcel.Status = ParcelStatus.Waiting;
                parcels.Add(parcel);
            }

            return repository.AddParcels(parcels);
        }

        public static IList<Parcel> EnsureMinimumDataset(IParcelRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            IList<Parcel> existing = repository.GetAllParcels();
            bool hasRequiredSizes = Enum.GetValues(typeof(ParcelSize)).Cast<ParcelSize>()
                .All(size => existing.Count(parcel => parcel.Size == size) >= 30);
            bool hasRequiredServices = Enum.GetValues(typeof(ParcelServiceType)).Cast<ParcelServiceType>()
                .All(service => existing.Count(parcel => parcel.ServiceType == service) >= 20);

            if (existing.Count < 100 || !hasRequiredSizes || !hasRequiredServices)
            {
                return GenerateDataset(repository);
            }

            return new List<Parcel>();
        }
    }
}
