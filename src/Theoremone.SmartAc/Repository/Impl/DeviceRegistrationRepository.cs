using Microsoft.EntityFrameworkCore;
using Theoremone.SmartAc.Data;
using Theoremone.SmartAc.Data.Models;

namespace Theoremone.SmartAc.Repository.Impl
{
    /// <summary>
    /// Maps the methods to retrieve data and persist data on DeviceRegistration entity.
    /// </summary>
    public class DeviceRegistrationRepository : IDeviceRegistrationRepository
    {
        private readonly SmartAcContext _db;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="db">The database connection.</param>
        public DeviceRegistrationRepository(SmartAcContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Update Device registration by inactivating it.
        /// </summary>
        /// <param name="deviceSerialNumber">The device serial number.</param>
        /// <param name="commit">True to persist the changes immediately.</param>
        /// <returns></returns>
        public async Task DeactivateRegistratiosBySerialNumber(string deviceSerialNumber, bool commit = true)
        {
            var registrations = _db.DeviceRegistrations
            .Where(registration => registration.DeviceSerialNumber == deviceSerialNumber && registration.Active);

            await foreach (var registration in registrations.AsAsyncEnumerable())
            {
                registration.Active = false;
            }
            if (commit)
            {
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Add a new registration.
        /// </summary>
        /// <param name="deviceRegistration">A new device registration.</param>
        /// <param name="commit"></param>
        /// <returns>True to persist the changes immediately.</returns>
        public async Task AddRegistration(DeviceRegistration deviceRegistration, bool commit = true)
        {
            await _db.DeviceRegistrations.AddAsync(deviceRegistration);

            if (commit)
            {
                await _db.SaveChangesAsync();
            }
        }
    }
}
