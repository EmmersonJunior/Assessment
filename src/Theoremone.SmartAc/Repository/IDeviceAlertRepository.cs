using Theoremone.SmartAc.Api.Models;
using Theoremone.SmartAc.Data.Models;

namespace Theoremone.SmartAc.Repository
{
    public interface IDeviceAlertRepository : IDisposable
    {
        /// <summary>
        /// Get Alerts using serial number in a time range.
        /// </summary>
        /// <param name="serialNumber">The device serial number.</param>
        /// <param name="rangeTime">The period to use the same alert instead of create a new one.</param>
        /// <returns>The device alert matching the parameters.</returns>
        Task<IList<DeviceAlert>> GetDeviceAlertsBySerialNumberInLastGivenMinutes(string serialNumber, DateTimeOffset rangeTime);

        /// <summary>
        /// Get a device reading by Id.
        /// </summary>
        /// <param name="id">Device reading Id.</param>
        /// <returns>A device Reading if found.</returns>
        Task<DeviceReading?> GetDeviceReadingById(int id);


        /// <summary>
        /// Insert a list of new alerts.
        /// </summary>
        /// <param name="deviceAlerts">The new instances of Device Alert.</param>
        /// <returns></returns>
        Task InserAlerts(IList<DeviceAlert> deviceAlerts);

        /// <summary>
        /// Commit at db. Save all changes at the database.
        /// </summary>
        /// <returns></returns>
        Task Commit();

        /// <summary>
        /// Get device alerts range based on serial number.
        /// </summary>
        /// <param name="pageNumber">The page to retrieve data.</param>
        /// <param name="pageSize">The page size of return.</param>
        /// <param name="serialNumber">The device serial number.</param>
        /// <returns>A list of alerts matching the serial number.</returns>
        Task<IList<DeviceAlert>> GetDeviceAlerts(int pageNumber, int pageSize, string serialNumber, AlertStatusSearchEnum status);

        /// <summary>
        /// Get the total amount of alerts for a device.
        /// </summary>
        /// <param name="serialNumber">The device serial number.</param>
        /// <param name="status">The status of an alert.</param>
        /// <returns>The total amount of alerts.</returns>
        Task<int> GetAlertsAmount(string serialNumber, AlertStatusSearchEnum status);

        Task<IList<DeviceAlert>> GetOldUnresolvedAlerts(string serialNumber, DateTimeOffset rangeTime);
    }
}