using Theoremone.SmartAc.Api.Models;
using Theoremone.SmartAc.Data.Models;

namespace Theoremone.SmartAc.Repository
{
    /// <summary>
    /// Maps the methods to retrieve data and persist data on DeviceReading entity.
    /// </summary>
    public interface IDeviceReadingRepository
    {
        /// <summary>
        /// Add new readings to a device.
        /// </summary>
        /// <param name="deviceReadings">The list of readings coming from a device.</param>
        /// <param name="commit">true to save the changes immediately.</param>
        /// <returns></returns>
        Task AddDevices(List<DeviceReading> deviceReadings, bool commit = true);
    }
}