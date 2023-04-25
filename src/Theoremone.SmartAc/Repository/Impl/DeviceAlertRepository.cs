using Microsoft.EntityFrameworkCore;
using Theoremone.SmartAc.Api.Models;
using Theoremone.SmartAc.Data;
using Theoremone.SmartAc.Data.Models;

namespace Theoremone.SmartAc.Repository.Impl
{
    public class DeviceAlertRepository : IDeviceAlertRepository
    {
        private readonly SmartAcContext _db;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="configuration">The interface to retrieve configuration props.</param>
        public DeviceAlertRepository(IConfiguration configuration)
        {
            _db = configuration.GetSmartAcContext();
        }

        /// <summary>
        /// To disconnect from DB.
        /// </summary>
        public void Dispose()
        {
            _db.Dispose();
        }

        /// <summary>
        /// Get Alerts using serial number in a time range.
        /// </summary>
        /// <param name="serialNumber">The device serial number.</param>
        /// <param name="rangeTime">The period to use the same alert instead of create a new one.</param>
        /// <returns>The device alert matching the parameters.</returns>
        public async Task<IList<DeviceAlert>> GetDeviceAlertsBySerialNumberInLastGivenMinutes(string serialNumber, DateTimeOffset rangeTime)
        {
            IQueryable<DeviceAlert> deviceAlerts = from dev in _db.Devices
                                                   join reg in _db.DeviceReadings on dev.SerialNumber equals reg.DeviceSerialNumber
                                                   join alert in _db.DeviceAlerts on reg.DeviceReadingId equals alert.DeviceReadingId
                                                   where dev.SerialNumber == serialNumber
                                                   && alert.DeviceAlertUpdateDate >= rangeTime
                                                   select alert;
            return await deviceAlerts.ToListAsync();
        }

        public async Task<IList<DeviceAlert>> GetOldUnresolvedAlerts(string serialNumber, DateTimeOffset rangeTime)
        {
            IQueryable<DeviceAlert> deviceAlerts = from dev in _db.Devices
                                                   join reg in _db.DeviceReadings on dev.SerialNumber equals reg.DeviceSerialNumber
                                                   join alert in _db.DeviceAlerts on reg.DeviceReadingId equals alert.DeviceReadingId
                                                   where dev.SerialNumber == serialNumber
                                                   && alert.DeviceAlertUpdateDate < rangeTime
                                                   && alert.AlertStatus == AlertStatusEnum.NEW
                                                   select alert;
            return await deviceAlerts.ToListAsync();
        }

        /// <summary>
        /// Get a device reading by Id.
        /// </summary>
        /// <param name="id">Device reading Id.</param>
        /// <returns>A device Reading if found.</returns>
        public async Task<DeviceReading?> GetDeviceReadingById(int id)
        {
            return await _db.DeviceReadings.FirstOrDefaultAsync(d => d.DeviceReadingId.Equals(id));
        }

        /// <summary>
        /// Insert a list of new alerts.
        /// </summary>
        /// <param name="deviceAlerts">The new instances of Device Alert.</param>
        /// <returns></returns>
        public async Task InserAlerts(IList<DeviceAlert> deviceAlerts)
        {
            await _db.DeviceAlerts.AddRangeAsync(deviceAlerts);
        }

        /// <summary>
        /// Commit at db. Save all changes at the database.
        /// </summary>
        /// <returns></returns>
        public async Task Commit()
        {
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Get device alerts range based on serial number.
        /// </summary>
        /// <param name="pageNumber">The page to retrieve data.</param>
        /// <param name="pageSize">The page size of return.</param>
        /// <param name="serialNumber">The device serial number.</param>
        /// <param name="status">The status of an alert.</param>
        /// <returns>A list of alerts matching the serial number.</returns>
        public async Task<IList<DeviceAlert>> GetDeviceAlerts(int pageNumber, int pageSize, string serialNumber, AlertStatusSearchEnum status)
        {
            IQueryable<DeviceAlert> queryableDeviceAlerts;
            if (status.Equals(AlertStatusSearchEnum.RESOLVED) || status.Equals(AlertStatusSearchEnum.NEW))
            {
                AlertStatusEnum statusEnum = (status.Equals(AlertStatusSearchEnum.RESOLVED) ? AlertStatusEnum.RESOLVED : AlertStatusEnum.NEW);
                queryableDeviceAlerts = GetDeviceAlersBySerialNumberAndStatus(serialNumber, statusEnum);
            }
            else
            {
                queryableDeviceAlerts = GetDeviceAlersBySerialNumber(serialNumber);
            }

            IList<DeviceAlert> deviceAlerts = await queryableDeviceAlerts
                .OrderByDescending(alert => alert.DeviceAlertDate)
                .OrderByDescending(alert => alert.DeviceAlertId)
                .Skip(pageNumber * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return deviceAlerts;
        }

        /// <summary>
        /// Get the total amount of alerts for a device.
        /// </summary>
        /// <param name="serialNumber">The device serial number.</param>
        /// <param name="status">The status of an alert.</param>
        /// <returns>The total amount of alerts.</returns>
        public async Task<int> GetAlertsAmount(string serialNumber, AlertStatusSearchEnum status)
        {
            IQueryable<DeviceAlert> queryableDeviceAlerts;
            if (status.Equals(AlertStatusSearchEnum.RESOLVED) || status.Equals(AlertStatusSearchEnum.NEW))
            {
                AlertStatusEnum statusEnum = (status.Equals(AlertStatusSearchEnum.RESOLVED) ? AlertStatusEnum.RESOLVED : AlertStatusEnum.NEW);
                queryableDeviceAlerts = GetDeviceAlersBySerialNumberAndStatus(serialNumber, statusEnum);
            }
            else
            {
                queryableDeviceAlerts = GetDeviceAlersBySerialNumber(serialNumber);
            }
            return await queryableDeviceAlerts.CountAsync();
        }

        private IQueryable<DeviceAlert> GetDeviceAlersBySerialNumberAndStatus(string serialNumber, AlertStatusEnum status)
        {
            return from dev in _db.Devices
                   join reg in _db.DeviceReadings on dev.SerialNumber equals reg.DeviceSerialNumber
                   join alert in _db.DeviceAlerts on reg.DeviceReadingId equals alert.DeviceReadingId
                   where dev.SerialNumber == serialNumber
                   && alert.AlertStatus == status
                   select alert;
        }

        private IQueryable<DeviceAlert> GetDeviceAlersBySerialNumber(string serialNumber)
        {
            return from dev in _db.Devices
                   join reg in _db.DeviceReadings on dev.SerialNumber equals reg.DeviceSerialNumber
                   join alert in _db.DeviceAlerts on reg.DeviceReadingId equals alert.DeviceReadingId
                   where dev.SerialNumber == serialNumber
                   select alert;
        }
    }
}
