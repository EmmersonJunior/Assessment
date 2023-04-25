using Theoremone.SmartAc.Data.Models;

namespace Theoremone.SmartAc.Repository
{
    /// <summary>
    /// Maps the methods to retrieve data and persist data on DeviceRegistration entity.
    /// </summary>
    public interface IDeviceRegistrationRepository
    {
        /// <summary>
        /// Add a new registration.
        /// </summary>
        /// <param name="deviceRegistration">A new device registration.</param>
        /// <param name="commit"></param>
        /// <returns>True to persist the changes immediately.</returns>
        Task AddRegistration(DeviceRegistration deviceRegistration, bool commit = false);

        /// <summary>
        /// Update Device registration by inactivating it.
        /// </summary>
        /// <param name="deviceSerialNumber">The device serial number.</param>
        /// <param name="commit">True to persist the changes immediately.</param>
        /// <returns></returns>
        Task DeactivateRegistratiosBySerialNumber(string deviceSerialNumber, bool commit = false);
    }
}