using Theoremone.SmartAc.Api.Models;
using Theoremone.SmartAc.Exceptions;

namespace Theoremone.SmartAc.Services
{
    public interface IDeviceIngestionService
    {
        /// <summary>
        /// Register a new device and generates a token for it.
        /// </summary>
        /// <param name="serialNumber">The device serial number.</param>
        /// <param name="sharedSecret">The device registration secret.</param>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <returns>A new token required for future transactions.</returns>
        /// <exception cref="UnauthorizedException">If the serial number does not match with database value.</exception>
        Task<string?> RegisterDevice(string serialNumber, string sharedSecret, string firmwareVersion);

        /// <summary>
        /// Process the sensor readings and persist the results.
        /// </summary>
        /// <param name="serialNumber">The device serial number.</param>
        /// <param name="deviceReadingRecords">The readings from device.</param>
        /// <returns></returns>
        Task AddSensorReadings(string serialNumber, IEnumerable<DeviceReadingRecord> deviceReadingRecords);
    }
}