using Theoremone.SmartAc.Data;
using Theoremone.SmartAc.Data.Models;

namespace Theoremone.SmartAc.Repository.Impl
{
    /// <summary>
    /// Maps the methods to retrieve data and persist data on DeviceReading entity.
    /// </summary>
    public class DeviceReadingRepository : IDeviceReadingRepository
    {
        private readonly SmartAcContext _db;

        public DeviceReadingRepository(SmartAcContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Add new readings to a device.
        /// </summary>
        /// <param name="deviceReadings">The list of readings coming from a device.</param>
        /// <param name="commit">true to save the changes immediately.</param>
        /// <returns></returns>
        public async Task AddDevices(List<DeviceReading> deviceReadings, bool commit = true)
        {
            await _db.DeviceReadings.AddRangeAsync(deviceReadings);

            if (commit)
            {
                await _db.SaveChangesAsync();
            }
        }
    }
}
