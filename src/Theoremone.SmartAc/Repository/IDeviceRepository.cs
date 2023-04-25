using SQLitePCL;
using Theoremone.SmartAc.Data.Models;

namespace Theoremone.SmartAc.Repository
{
    /// <summary>
    /// Maps the methods to retrieve data and persist data on Device entity.
    /// </summary>
    public interface IDeviceRepository
    {
        /// <summary>
        /// Get Device using serial number and shared secret.
        /// </summary>
        /// <param name="serialNumber">The device serial number.</param>
        /// <param name="sharedSecret">The device shared secret.</param>
        /// <returns></returns>
        Task<Device?> GetDeviceBySerialNumberAndSharedSecret(string serialNumber, string sharedSecret);

        /// <summary>
        /// Update a device detail.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="firmwareVersion">The device firmware verison.</param>
        /// <param name="deviceRegistration">The device registration.</param>
        /// <param name="commit"></param>
        /// <returns>True to persist the changes immediately.</returns>
        Task UpdateDetails(Device device, string firmwareVersion, DeviceRegistration deviceRegistration, bool commit = false);
    }
}