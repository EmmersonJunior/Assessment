using Microsoft.EntityFrameworkCore;
using Theoremone.SmartAc.Data;
using Theoremone.SmartAc.Data.Models;

namespace Theoremone.SmartAc.Repository.Impl
{
    /// <summary>
    /// Maps the methods to retrieve data and persist data on Device entity.
    /// </summary>
    public class DeviceRepository : IDeviceRepository
    {
        private readonly SmartAcContext _db;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="db">The database connection.</param>
        public DeviceRepository(SmartAcContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Get Device using serial number and shared secret.
        /// </summary>
        /// <param name="serialNumber">The device serial number.</param>
        /// <param name="sharedSecret">The device shared secret.</param>
        /// <returns></returns>
        public async Task<Device?> GetDeviceBySerialNumberAndSharedSecret(string serialNumber, string sharedSecret)
        {
            Device? device = await _db.Devices
            .Where(device => device.SerialNumber.Equals(serialNumber) && device.SharedSecret.Equals(sharedSecret))
            .FirstOrDefaultAsync();

            return device;
        }

        /// <summary>
        /// Update a device detail.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="firmwareVersion">The device firmware verison.</param>
        /// <param name="deviceRegistration">The device registration.</param>
        /// <param name="commit"></param>
        /// <returns>True to persist the changes immediately.</returns>
        public async Task UpdateDetails(Device device, string firmwareVersion, DeviceRegistration deviceRegistration, bool commit = true)
        {
            device.FirmwareVersion = firmwareVersion;
            device.FirstRegistrationDate ??= deviceRegistration.RegistrationDate;
            device.LastRegistrationDate = deviceRegistration.RegistrationDate;

            if (commit)
            {
                await _db.SaveChangesAsync();
            }
        }
    }
}
