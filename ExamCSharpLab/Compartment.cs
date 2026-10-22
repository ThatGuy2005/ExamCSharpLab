using System;
using System.Collections.Generic;
using System.Linq;

namespace ExamCSharpLab
{
    public sealed class LockerCompartment
    {
        public int Id { get; private set; }
        public ParcelSize Size { get; private set; }

        public LockerCompartment(int id, ParcelSize size)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException("id");
            }

            Id = id;
            Size = size;
        }
    }

    public sealed class LockerUnit
    {
        private readonly List<LockerCompartment> compartments;

        public LockerUnit(IEnumerable<LockerCompartment> compartments)
        {
            if (compartments == null)
            {
                throw new ArgumentNullException("compartments");
            }

            this.compartments = compartments.OrderBy(c => c.Id).ToList();
            if (this.compartments.Select(c => c.Id).Distinct().Count() != this.compartments.Count)
            {
                throw new ArgumentException("Compartment IDs must be unique.", "compartments");
            }
        }

        public IReadOnlyList<LockerCompartment> Compartments
        {
            get { return compartments.AsReadOnly(); }
        }

        public static LockerUnit CreateStandardUnit()
        {
            var compartments = new List<LockerCompartment>();
            int id = 1;
            foreach (ParcelSize size in Enum.GetValues(typeof(ParcelSize)))
            {
                for (int i = 0; i < 8; i++)
                {
                    compartments.Add(new LockerCompartment(id++, size));
                }
            }

            return new LockerUnit(compartments);
        }
    }
}