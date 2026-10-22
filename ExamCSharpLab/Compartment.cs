using System;
using System.Collections.Generic;
using System.Linq;

namespace ExamCSharpLab
{
    /// <summary>A physical locker box with a fixed identifier and capacity.</summary>
    public sealed class LockerCompartment
    {
        public int Id { get; private set; }
        public ParcelSize Size { get; private set; }

        /// <summary>Creates a compartment with a positive database ID and fixed size.</summary>
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

    /// <summary>
    /// Owns an ordered collection of compartments, preventing duplicate compartment IDs within one unit.
    /// </summary>
    public sealed class LockerUnit
    {
        private readonly List<LockerCompartment> compartments;

        /// <summary>Copies and sorts the supplied compartments, then verifies their IDs are unique.</summary>
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

        /// <summary>Read-only view of this unit's compartments ordered by ID.</summary>
        public IReadOnlyList<LockerCompartment> Compartments
        {
            get { return compartments.AsReadOnly(); }
        }

        /// <summary>Creates the exam-standard unit containing eight compartments of each size.</summary>
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