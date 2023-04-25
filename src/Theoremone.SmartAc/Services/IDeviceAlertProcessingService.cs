using Theoremone.SmartAc.Api.Models;
using Theoremone.SmartAc.Data.Models;

namespace Theoremone.SmartAc.Services
{
    /// <summary>
    /// Holds the methods to process the device reading generating alerts and storing them.
    /// </summary>
    public interface IDeviceAlertProcessingService
    {
        /// <summary>
        /// Allow pushing records to the queue. 
        /// </summary>
        /// <param name="deviceReadingRecords">Set of device readings IDs.</param>
        void PushDeviceReadingEvent(IEnumerable<int> deviceReadingRecords);

        /// <summary>
        /// Check sensors readings and create allerts
        /// </summary>
        /// <param name="deviceReading">The representation of a device reading.</param>
        /// <returns>All alerts for a reading.</returns>
        IList<DeviceAlert> CheckSensors(DeviceReading deviceReading);

        /// <summary>
        /// Get a page of Alert response.
        /// </summary>
        /// <param name="serialNumber">The device serial number.</param>
        /// <param name="paginationFilter">The pagination parameters.</param>
        /// <returns>A page of alerts for a device.</returns>
        Task<PagedResponse<IList<AlertResponse>>> GetAlerts(string serialNumber, PaginationFilter paginationFilter, AlertStatusSearchEnum status);
    }
}