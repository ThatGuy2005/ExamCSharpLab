using System;

namespace ExamCSharpLab
{
    /// <summary>
    /// Parcel sizes are numbered in increasing capacity order so numeric comparisons support best-fit allocation.
    /// </summary>
    public enum ParcelSize
    {
        Small = 1,
        Medium = 2,
        Large = 3
    }

    /// <summary>Lifecycle states persisted for each parcel.</summary>
    public enum ParcelStatus
    {
        Waiting,
        Stored,
        Collected
    }

    /// <summary>The service category determines a parcel's processing priority and delay.</summary>
    public enum ParcelServiceType
    {
        Standard,
        Express
    }

    /// <summary>
    /// Common parcel data. Derived classes provide their service category and simulated processing delay.
    /// </summary>
    public abstract class Parcel
    {
        /// <summary>Database-generated identifier, or imported identifier for XML data.</summary>
        public int Id { get; set; }

        /// <summary>Name of the person receiving the parcel.</summary>
        public string Recipient { get; set; }

        /// <summary>Required minimum compartment capacity for this parcel.</summary>
        public ParcelSize Size { get; set; }

        /// <summary>Current lifecycle state; only Stored parcels occupy a compartment.</summary>
        public ParcelStatus Status { get; set; }

        /// <summary>Assigned compartment, retained after collection as history.</summary>
        public int? CompartmentId { get; set; }

        /// <summary>Pickup credential assigned when the parcel becomes Stored.</summary>
        public string PickupCode { get; set; }

        /// <summary>Service type used to prioritize Waiting parcels.</summary>
        public abstract ParcelServiceType ServiceType { get; }

        /// <summary>Returns the simulated processing time for this parcel type.</summary>
        public abstract int GetProcessingDelayMs();
    }

    /// <summary>Standard-service parcel with the longer processing delay.</summary>
    public sealed class StandardParcel : Parcel
    {
        public override ParcelServiceType ServiceType { get { return ParcelServiceType.Standard; } }

        /// <summary>Returns the Standard service's simulated delay.</summary>
        public override int GetProcessingDelayMs()
        {
            return 600;
        }
    }

    /// <summary>Express-service parcel with the shorter processing delay.</summary>
    public sealed class ExpressParcel : Parcel
    {
        public override ParcelServiceType ServiceType { get { return ParcelServiceType.Express; } }

        /// <summary>Returns the Express service's simulated delay.</summary>
        public override int GetProcessingDelayMs()
        {
            return 300;
        }
    }
}