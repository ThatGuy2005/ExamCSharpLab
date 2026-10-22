using System;

namespace ExamCSharpLab
{
    public enum ParcelSize
    {
        Small = 1,
        Medium = 2,
        Large = 3
    }

    public enum ParcelStatus
    {
        Waiting,
        Stored,
        Collected
    }

    public enum ParcelServiceType
    {
        Standard,
        Express
    }

    public abstract class Parcel
    {
        public int Id { get; set; }
        public string Recipient { get; set; }
        public ParcelSize Size { get; set; }
        public ParcelStatus Status { get; set; }
        public int? CompartmentId { get; set; }
        public string PickupCode { get; set; }
        public abstract ParcelServiceType ServiceType { get; }
        public abstract int GetProcessingDelayMs();
    }

    public sealed class StandardParcel : Parcel
    {
        public override ParcelServiceType ServiceType { get { return ParcelServiceType.Standard; } }

        public override int GetProcessingDelayMs()
        {
            return 600;
        }
    }

    public sealed class ExpressParcel : Parcel
    {
        public override ParcelServiceType ServiceType { get { return ParcelServiceType.Express; } }

        public override int GetProcessingDelayMs()
        {
            return 300;
        }
    }
}