using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace ExamCSharpLab
{
    public sealed class SqliteParcelRepository : IParcelRepository
    {
        private readonly string connectionString;

        public string DatabasePath { get; private set; }

        public SqliteParcelRepository(string databasePath = null)
        {
            DatabasePath = Path.GetFullPath(databasePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "smartlocker.db"));
            string directory = Path.GetDirectoryName(DatabasePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            connectionString = "Data Source=" + DatabasePath + ";Version=3;";
        }

        public void Initialize()
        {
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "CREATE TABLE IF NOT EXISTS Compartments (" +
                    "Id INTEGER PRIMARY KEY, Size TEXT NOT NULL CHECK (Size IN ('Small','Medium','Large')));" +
                    "CREATE TABLE IF NOT EXISTS Parcels (" +
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "Recipient TEXT NOT NULL CHECK (length(trim(Recipient)) > 0), " +
                    "Size TEXT NOT NULL CHECK (Size IN ('Small','Medium','Large')), " +
                    "ServiceType TEXT NOT NULL CHECK (ServiceType IN ('Standard','Express')), " +
                    "Status TEXT NOT NULL CHECK (Status IN ('Waiting','Stored','Collected')), " +
                    "CompartmentId INTEGER NULL REFERENCES Compartments(Id), " +
                    "PickupCode TEXT NULL);" +
                    "CREATE UNIQUE INDEX IF NOT EXISTS UX_Parcels_StoredCompartment " +
                    "ON Parcels(CompartmentId) WHERE Status = 'Stored';" +
                    "CREATE UNIQUE INDEX IF NOT EXISTS UX_Parcels_StoredPickupCode " +
                    "ON Parcels(PickupCode) WHERE Status = 'Stored' AND PickupCode IS NOT NULL;";
                command.ExecuteNonQuery();
            }

            using (var connection = OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                using (var countCommand = connection.CreateCommand())
                {
                    countCommand.Transaction = transaction;
                    countCommand.CommandText = "SELECT COUNT(*) FROM Compartments;";
                    if (Convert.ToInt32(countCommand.ExecuteScalar()) == 0)
                    {
                        foreach (var compartment in LockerUnit.CreateStandardUnit().Compartments)
                        {
                            using (var insertCommand = connection.CreateCommand())
                            {
                                insertCommand.Transaction = transaction;
                                insertCommand.CommandText = "INSERT INTO Compartments (Id, Size) VALUES (@id, @size);";
                                insertCommand.Parameters.AddWithValue("@id", compartment.Id);
                                insertCommand.Parameters.AddWithValue("@size", compartment.Size.ToString());
                                insertCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }

                transaction.Commit();
            }
        }

        public int AddParcel(Parcel parcel)
        {
            ValidateParcel(parcel);
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO Parcels (Recipient, Size, ServiceType, Status, CompartmentId, PickupCode) " +
                                      "VALUES (@recipient, @size, @service, 'Waiting', NULL, NULL); SELECT last_insert_rowid();";
                command.Parameters.AddWithValue("@recipient", parcel.Recipient.Trim());
                command.Parameters.AddWithValue("@size", parcel.Size.ToString());
                command.Parameters.AddWithValue("@service", parcel.ServiceType.ToString());
                parcel.Id = Convert.ToInt32(command.ExecuteScalar());
                parcel.Status = ParcelStatus.Waiting;
                parcel.CompartmentId = null;
                parcel.PickupCode = null;
                return parcel.Id;
            }
        }

        public IList<Parcel> AddParcels(IEnumerable<Parcel> parcels)
        {
            if (parcels == null)
            {
                throw new ArgumentNullException("parcels");
            }

            var list = parcels.ToList();
            foreach (var parcel in list)
            {
                ValidateParcel(parcel);
            }

            using (var connection = OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                foreach (var parcel in list)
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = "INSERT INTO Parcels (Recipient, Size, ServiceType, Status, CompartmentId, PickupCode) " +
                                              "VALUES (@recipient, @size, @service, 'Waiting', NULL, NULL); SELECT last_insert_rowid();";
                        command.Parameters.AddWithValue("@recipient", parcel.Recipient.Trim());
                        command.Parameters.AddWithValue("@size", parcel.Size.ToString());
                        command.Parameters.AddWithValue("@service", parcel.ServiceType.ToString());
                        parcel.Id = Convert.ToInt32(command.ExecuteScalar());
                        parcel.Status = ParcelStatus.Waiting;
                        parcel.CompartmentId = null;
                        parcel.PickupCode = null;
                    }
                }

                transaction.Commit();
            }

            return list;
        }

        public void ReplaceAllData(IEnumerable<LockerCompartment> compartments, IEnumerable<Parcel> parcels)
        {
            if (compartments == null)
            {
                throw new ArgumentNullException("compartments");
            }

            if (parcels == null)
            {
                throw new ArgumentNullException("parcels");
            }

            List<LockerCompartment> compartmentList = compartments.ToList();
            List<Parcel> parcelList = parcels.ToList();
            var compartmentsById = compartmentList.ToDictionary(compartment => compartment.Id);
            if (compartmentList.Count == 0 || compartmentList.Count != compartmentsById.Count)
            {
                throw new ArgumentException("The import must contain compartments with distinct IDs.", "compartments");
            }

            var parcelIds = new HashSet<int>();
            var storedCompartmentIds = new HashSet<int>();
            var storedPickupCodes = new HashSet<string>(StringComparer.Ordinal);
            foreach (Parcel parcel in parcelList)
            {
                ValidateParcel(parcel);
                if (parcel.Id <= 0 || !parcelIds.Add(parcel.Id))
                {
                    throw new ArgumentException("Imported parcel IDs must be positive and unique.", "parcels");
                }

                if (!Enum.IsDefined(typeof(ParcelStatus), parcel.Status))
                {
                    throw new ArgumentException("Imported parcel status is invalid.", "parcels");
                }

                if (parcel.Status == ParcelStatus.Stored)
                {
                    if (!parcel.CompartmentId.HasValue || string.IsNullOrWhiteSpace(parcel.PickupCode) ||
                        parcel.PickupCode.Length != 6 || parcel.PickupCode.Any(character => !char.IsDigit(character)))
                    {
                        throw new ArgumentException("Stored parcels require a compartment and a six-digit pickup code.", "parcels");
                    }

                    LockerCompartment compartment;
                    if (!compartmentsById.TryGetValue(parcel.CompartmentId.Value, out compartment) ||
                        (int)compartment.Size < (int)parcel.Size ||
                        !storedCompartmentIds.Add(parcel.CompartmentId.Value) ||
                        !storedPickupCodes.Add(parcel.PickupCode))
                    {
                        throw new ArgumentException("Stored parcel assignments must use a compatible, unique compartment and pickup code.", "parcels");
                    }
                }
            }

            using (var connection = OpenConnection())
            using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = "DELETE FROM Parcels; DELETE FROM Compartments;";
                    command.ExecuteNonQuery();
                }

                foreach (LockerCompartment compartment in compartmentList)
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = "INSERT INTO Compartments (Id, Size) VALUES (@id, @size);";
                        command.Parameters.AddWithValue("@id", compartment.Id);
                        command.Parameters.AddWithValue("@size", compartment.Size.ToString());
                        command.ExecuteNonQuery();
                    }
                }

                foreach (Parcel parcel in parcelList)
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = "INSERT INTO Parcels (Id, Recipient, Size, ServiceType, Status, CompartmentId, PickupCode) " +
                            "VALUES (@id, @recipient, @size, @service, @status, @compartmentId, @pickupCode);";
                        command.Parameters.AddWithValue("@id", parcel.Id);
                        command.Parameters.AddWithValue("@recipient", parcel.Recipient.Trim());
                        command.Parameters.AddWithValue("@size", parcel.Size.ToString());
                        command.Parameters.AddWithValue("@service", parcel.ServiceType.ToString());
                        command.Parameters.AddWithValue("@status", parcel.Status.ToString());
                        command.Parameters.AddWithValue("@compartmentId", (object)parcel.CompartmentId ?? DBNull.Value);
                        command.Parameters.AddWithValue("@pickupCode", (object)parcel.PickupCode ?? DBNull.Value);
                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }

        public IList<Parcel> GetAllParcels()
        {
            var parcels = new List<Parcel>();
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Id, Recipient, Size, ServiceType, Status, CompartmentId, PickupCode FROM Parcels ORDER BY Id;";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var service = ParseEnum<ParcelServiceType>(reader.GetString(3));
                        Parcel parcel = service == ParcelServiceType.Express ? (Parcel)new ExpressParcel() : new StandardParcel();
                        parcel.Id = reader.GetInt32(0);
                        parcel.Recipient = reader.GetString(1);
                        parcel.Size = ParseEnum<ParcelSize>(reader.GetString(2));
                        parcel.Status = ParseEnum<ParcelStatus>(reader.GetString(4));
                        parcel.CompartmentId = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5);
                        parcel.PickupCode = reader.IsDBNull(6) ? null : reader.GetString(6);
                        parcels.Add(parcel);
                    }
                }
            }

            return parcels;
        }

        public IList<LockerCompartment> GetCompartments()
        {
            var compartments = new List<LockerCompartment>();
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Id, Size FROM Compartments ORDER BY Id;";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        compartments.Add(new LockerCompartment(reader.GetInt32(0), ParseEnum<ParcelSize>(reader.GetString(1))));
                    }
                }
            }

            return compartments;
        }

        public bool TryStoreParcel(int parcelId, int compartmentId, string pickupCode)
        {
            if (string.IsNullOrWhiteSpace(pickupCode) || pickupCode.Length != 6 || pickupCode.Any(c => !char.IsDigit(c)))
            {
                throw new ArgumentException("Pickup codes must contain exactly six digits.", "pickupCode");
            }

            using (var connection = OpenConnection())
            using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = "UPDATE Parcels SET Status = 'Stored', CompartmentId = @compartmentId, PickupCode = @pickupCode " +
                    "WHERE Id = @parcelId AND Status = 'Waiting' " +
                    "AND EXISTS (SELECT 1 FROM Compartments WHERE Id = @compartmentId) " +
                    "AND NOT EXISTS (SELECT 1 FROM Parcels WHERE Status = 'Stored' AND CompartmentId = @compartmentId) " +
                    "AND NOT EXISTS (SELECT 1 FROM Parcels WHERE Status = 'Stored' AND PickupCode = @pickupCode);";
                command.Parameters.AddWithValue("@compartmentId", compartmentId);
                command.Parameters.AddWithValue("@pickupCode", pickupCode);
                command.Parameters.AddWithValue("@parcelId", parcelId);
                bool stored = command.ExecuteNonQuery() == 1;
                transaction.Commit();
                return stored;
            }
        }

        public Parcel CollectByPickupCode(string pickupCode)
        {
            if (string.IsNullOrWhiteSpace(pickupCode))
            {
                return null;
            }

            using (var connection = OpenConnection())
            using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                int parcelId;
                using (var select = connection.CreateCommand())
                {
                    select.Transaction = transaction;
                    select.CommandText = "SELECT Id FROM Parcels WHERE PickupCode = @code AND Status = 'Stored';";
                    select.Parameters.AddWithValue("@code", pickupCode.Trim());
                    object result = select.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                    {
                        transaction.Rollback();
                        return null;
                    }

                    parcelId = Convert.ToInt32(result);
                }

                using (var update = connection.CreateCommand())
                {
                    update.Transaction = transaction;
                    update.CommandText = "UPDATE Parcels SET Status = 'Collected' WHERE Id = @id AND Status = 'Stored';";
                    update.Parameters.AddWithValue("@id", parcelId);
                    if (update.ExecuteNonQuery() != 1)
                    {
                        transaction.Rollback();
                        return null;
                    }
                }

                transaction.Commit();
                return GetAllParcels().SingleOrDefault(p => p.Id == parcelId);
            }
        }

        private SQLiteConnection OpenConnection()
        {
            var connection = new SQLiteConnection(connectionString);
            connection.Open();
            return connection;
        }

        private static void ValidateParcel(Parcel parcel)
        {
            if (parcel == null)
            {
                throw new ArgumentNullException("parcel");
            }

            if (string.IsNullOrWhiteSpace(parcel.Recipient))
            {
                throw new ArgumentException("Recipient is required.", "parcel");
            }

            if (!Enum.IsDefined(typeof(ParcelSize), parcel.Size) || !Enum.IsDefined(typeof(ParcelServiceType), parcel.ServiceType))
            {
                throw new ArgumentException("Parcel size or service type is invalid.", "parcel");
            }
        }

        private static T ParseEnum<T>(string value) where T : struct
        {
            return (T)Enum.Parse(typeof(T), value, false);
        }
    }
}
