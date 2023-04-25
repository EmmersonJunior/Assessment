using FluentScheduler;
using Theoremone.SmartAc.Api.Models;
using Theoremone.SmartAc.Data.Migrations.Sqlite;
using Theoremone.SmartAc.Data.Models;
using Theoremone.SmartAc.Repository;

namespace Theoremone.SmartAc.Services.Impl
{
    /// <summary>
    /// Holds the methods to process the device reading generating alerts and storing them.
    /// </summary>
    public class DeviceAlertProcessingService : Registry, IDeviceAlertProcessingService
    {
        #region constants
        private readonly decimal _humidityMax;
        private readonly decimal _humidityMin;
        private readonly decimal _tempMax;
        private readonly decimal _tempMin;
        private readonly decimal _coMax;
        private readonly decimal _coMin;
        private readonly decimal _coDangerousRange;
        private readonly int _messagesPerIteration;
        private readonly int _minutesToNewAlert;
        #endregion

        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private static Queue<int> deviceReadingRecordsQueue = new Queue<int>();
        private static readonly object queueLock = new();


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="configuration">The configuration properties holding database information.</param>
        /// <param name="logger">Loggin element.</param>
        public DeviceAlertProcessingService(IConfiguration configuration, ILogger<DeviceAlertProcessingService> logger)
        {
            _configuration = configuration;
            _humidityMax = GetLimit(configuration, "Limits:HumidityMax");
            _humidityMin = GetLimit(configuration, "Limits:HumidityMin");
            _tempMax = GetLimit(configuration, "Limits:TempMax");
            _tempMin = GetLimit(configuration, "Limits:TempMin");
            _coMax = GetLimit(configuration, "Limits:CoMax");
            _coMin = GetLimit(configuration, "Limits:CoMin");
            _coDangerousRange = GetLimit(configuration, "Limits:CoDangerousRange");
            _messagesPerIteration = configuration.GetValue<int>("Queue:MessagesPerIteration");
            _minutesToNewAlert = configuration.GetValue<int>("Queue:MinutesToNewAlert");
            _logger = logger;
            Schedule(async () => await ProcessAlerts()).ToRunEvery(configuration.GetValue<int>("Queue:Delay")).Seconds();
            JobManager.Initialize(this);
        }

        private decimal GetLimit(IConfiguration configuration, string key)
        {
            string value = configuration.GetValue<string>(key);

            return decimal.Parse(value);
        }


        /// <summary>
        /// Allow pushing records to the in memory queue. 
        /// </summary>
        /// <param name="deviceReadingRecords">Set of device readings IDs.</param>
        public void PushDeviceReadingEvent(IEnumerable<int> deviceReadingRecords)
        {
            lock (queueLock)
            {
                deviceReadingRecordsQueue.EnsureCapacity(deviceReadingRecords.Count());
                deviceReadingRecords
                    .ToList()
                    .ForEach(d => deviceReadingRecordsQueue.Enqueue(d));
            }
        }

        private IList<int> SafeDequeue()
        {
            IList<int> ids = new List<int>();
            lock (queueLock)
            {
                int count = deviceReadingRecordsQueue.Count;
                if (count > 0){
                    int range = count > _messagesPerIteration
                    ? _messagesPerIteration : count;

                    foreach (var _ in Enumerable.Range(1, range))
                    {
                        ids.Add(deviceReadingRecordsQueue.Dequeue());
                    }
                }
            }
            return ids;
        }

        /// <summary>
        /// This is the method managed by Scheduler and runs in a given interval.
        /// If the queue has messages these are processed and the alerts generated and persited on database.
        /// Five types of alerts are generated: TEMP_OUT_OF_RANGE, HUMIDITY_OUT_OF_RANGE, CO_OUT_OF_RANGE, DANGEROUS_CO_LEVELS, POOR_HEALTH
        /// </summary>
        private async Task ProcessAlerts()
        {
            IList<int> ids = SafeDequeue();
            if (ids.Count > 0)
            {
                using (IDeviceAlertRepository deviceAlertRepository = _configuration.GetDeviceAlertRepository())
                {
                    foreach (int id in ids)
                    {
                        DeviceReading? deviceReading = await deviceAlertRepository.GetDeviceReadingById(id);
                        if (deviceReading != null)
                        {
                            IList<DeviceAlert> newDeviceAlerts = CheckSensors(deviceReading);
                            await SolveOldAlerts(deviceAlertRepository, deviceReading, newDeviceAlerts, deviceReading.RecordedDateTime.AddMinutes(_minutesToNewAlert * -1));
                            IList<DeviceAlert> oldDeviceAlerts = await deviceAlertRepository
                                .GetDeviceAlertsBySerialNumberInLastGivenMinutes(deviceReading.DeviceSerialNumber, deviceReading.RecordedDateTime.AddMinutes(_minutesToNewAlert * -1));
                            await MergeNewAlertsWithOld(deviceAlertRepository, oldDeviceAlerts, newDeviceAlerts, deviceReading.RecordedDateTime, deviceReading);
                        }
                        else
                        {
                            _logger.LogWarning($"Any record found to the reading Id: [{id}]");
                        }
                        await deviceAlertRepository.Commit();
                    }
                }
            }
        }

        public async Task SolveOldAlerts(IDeviceAlertRepository deviceAlertRepository, DeviceReading deviceReading, IList<DeviceAlert> newDeviceAlerts, DateTimeOffset range)
        {
            IList<AlertTypeEnum> alertPossibleTypes = Enum.GetValues(typeof(AlertTypeEnum)).OfType<AlertTypeEnum>().ToList();
            IList<AlertTypeEnum> typesToResolve = alertPossibleTypes.Where(alert => !newDeviceAlerts.Select(al => al.AlertType).Contains(alert)).ToList();
            IList<DeviceAlert> oldDeviceAlerts = await deviceAlertRepository.GetOldUnresolvedAlerts(deviceReading.DeviceSerialNumber, range);

            foreach (DeviceAlert deviceAlert in oldDeviceAlerts)
            {
                if (typesToResolve.Contains(deviceAlert.AlertType) && deviceReading.RecordedDateTime > deviceAlert.DeviceAlertUpdateDate)
                {
                    deviceAlert.AlertStatus = AlertStatusEnum.RESOLVED;
                    deviceAlert.AlertUpdateTimes += 1;
                }
            }
        }

        /// <summary>
        /// If new reading is showing errors the past errors needs to be merged with the new ones to avoid unecessary data.
        /// </summary>
        /// <param name="deviceAlertRepository">Manage the device alert entity.</param>
        /// <param name="oldDeviceAlerts">The existing alerts within the timeframe.</param>
        /// <param name="newDeviceAlerts">The generated Device Alerts for a reading.</param>
        /// <param name="deviceReadingDate">The reading date and time.</param>
        public async Task MergeNewAlertsWithOld(IDeviceAlertRepository deviceAlertRepository, IList<DeviceAlert> oldDeviceAlerts, IList<DeviceAlert> newDeviceAlerts, DateTimeOffset deviceReadingDate, DeviceReading deviceReading)
        {
            if (oldDeviceAlerts != null && oldDeviceAlerts.Any())
            {
                foreach (DeviceAlert alert in oldDeviceAlerts)
                {
                    alert.AlertUpdateTimes += 1;
                    AlertStatusEnum status = newDeviceAlerts.Any(d => d.AlertType.Equals(alert.AlertType)) ? AlertStatusEnum.NEW : AlertStatusEnum.RESOLVED;
                    alert.DeviceAlertUpdateDate = alert.DeviceAlertUpdateDate < deviceReadingDate &&
                        (status.Equals(AlertStatusEnum.NEW) || !alert.AlertStatus.Equals(status))
                        ? deviceReadingDate : alert.DeviceAlertUpdateDate;
                    alert.AlertStatus = status;

                    if (CheckReadingOutsideLimits(deviceReading, alert.AlertType) && !alert.AlertType.Equals(AlertTypeEnum.POOR_HEALTH))
                    {
                        alert.MinReading = deviceReading.DefineMinReading(alert.MinReading, alert.AlertType);
                        alert.MaxReading = deviceReading.DefineMaxReading(alert.MaxReading, alert.AlertType);
                    }
                }
                IList<DeviceAlert> newDeviceAlertsToBeInserteds = new List<DeviceAlert>();

                foreach (DeviceAlert alert in newDeviceAlerts)
                {
                    if (!oldDeviceAlerts.Any(d => d.AlertType.Equals(alert.AlertType)))
                    {
                        newDeviceAlertsToBeInserteds.Add(alert);
                    }
                }
                await deviceAlertRepository.InserAlerts(newDeviceAlertsToBeInserteds);
            }
            else
            {
                await deviceAlertRepository.InserAlerts(newDeviceAlerts);
            }
        }

        private bool CheckReadingOutsideLimits(DeviceReading deviceReading, AlertTypeEnum type)
        {
            switch (type)
            {
                case AlertTypeEnum.DANGEROUS_CO_LEVELS:
                case AlertTypeEnum.CO_OUT_OF_RANGE:
                    return deviceReading.CarbonMonoxide > _coMax || deviceReading.CarbonMonoxide < _coMin || deviceReading.CarbonMonoxide > _coDangerousRange;
                case AlertTypeEnum.TEMP_OUT_OF_RANGE:
                    return deviceReading.Temperature > _tempMax || deviceReading.Temperature < _tempMin;
                case AlertTypeEnum.HUMIDITY_OUT_OF_RANGE:
                    return deviceReading.Humidity > _humidityMax || deviceReading.Humidity < _humidityMin;

                default: return false;
            }
        }

        /// <summary>
        /// Check sensors readings and create allerts.
        /// </summary>
        /// <param name="deviceReading">The representation of a device reading.</param>
        /// <returns>All alerts for a reading.</returns>
        public IList<DeviceAlert> CheckSensors(DeviceReading deviceReading)
        {
            IList<DeviceAlert> deviceAlerts = new List<DeviceAlert>();

            if (CheckCoDangerLevel(deviceReading))
            {
                string description = "CO value has exceeded danger limit";
                DeviceAlert deviceAlert = new DeviceAlert(AlertTypeEnum.DANGEROUS_CO_LEVELS, deviceReading, description, deviceReading.CarbonMonoxide);
                deviceAlerts.Add(deviceAlert);
                _logger.LogInformation($"The device reading Id: [{deviceReading.DeviceReadingId}] showed an error: [{description}]");
            }
            if (CheckCoOutRange(deviceReading))
            {
                string description = "Sensor carbon monoxide reported out of range value";
                DeviceAlert deviceAlert = new DeviceAlert(AlertTypeEnum.CO_OUT_OF_RANGE, deviceReading, description, deviceReading.CarbonMonoxide);
                deviceAlerts.Add(deviceAlert);
                _logger.LogInformation($"The device reading Id: [{deviceReading.DeviceReadingId}] showed an error: [{description}]");
            }
            if (CheckHealth(deviceReading))
            {
                string description = $"Device is reporting health problem: {deviceReading.Health}";
                DeviceAlert deviceAlert = new DeviceAlert(AlertTypeEnum.POOR_HEALTH, deviceReading, description);
                deviceAlerts.Add(deviceAlert);
                _logger.LogInformation($"The device reading Id: [{deviceReading.DeviceReadingId}] showed an error: [{description}]");
            }
            if (CheckHumidityOutRange(deviceReading))
            {
                string description = "Sensor humidity reported out of range value";
                DeviceAlert deviceAlert = new DeviceAlert(AlertTypeEnum.HUMIDITY_OUT_OF_RANGE, deviceReading, description, deviceReading.Humidity);
                deviceAlerts.Add(deviceAlert);
                _logger.LogInformation($"The device reading Id: [{deviceReading.DeviceReadingId}] showed an error: [{description}]");
            }
            if (CheckTempOutRange(deviceReading))
            {
                string description = "Sensor temperature reported out of range value";
                DeviceAlert deviceAlert = new DeviceAlert(AlertTypeEnum.TEMP_OUT_OF_RANGE, deviceReading, description, deviceReading.Temperature);
                deviceAlerts.Add(deviceAlert);
                _logger.LogInformation($"The device reading Id: [{deviceReading.DeviceReadingId}] showed an error: [{description}]");
            }
            if (!deviceAlerts.Any())
            {
                _logger.LogInformation($"The device reading Id: [{deviceReading.DeviceReadingId}] did not show any error");
            }
            return deviceAlerts;
        }

        private bool CheckHumidityOutRange(DeviceReading device)
        {
            return device.Humidity < _humidityMin || device.Humidity > _humidityMax;
        }
        private bool CheckTempOutRange(DeviceReading device)
        {
            return device.Temperature < _tempMin || device.Temperature > _tempMax;
        }
        private bool CheckCoOutRange(DeviceReading device)
        {
            return device.CarbonMonoxide < _coMin || device.CarbonMonoxide > _coMax;
        }
        private bool CheckCoDangerLevel(DeviceReading device)
        {
            return device.CarbonMonoxide > _coDangerousRange;
        }
        private bool CheckHealth(DeviceReading device)
        {
            return !device.Health.Equals(DeviceHealth.Ok);
        }

        /// <summary>
        /// Get a page of Alert response.
        /// </summary>
        /// <param name="serialNumber">The device serial number.</param>
        /// <param name="paginationFilter">The pagination parameters.</param>
        /// <param name="status">The possible status for search.</param>
        /// <returns>A page of alerts for a device.</returns>
        public async Task<PagedResponse<IList<AlertResponse>>> GetAlerts(string serialNumber, PaginationFilter paginationFilter, AlertStatusSearchEnum status)
        {
            using (IDeviceAlertRepository deviceAlertRepository = _configuration.GetDeviceAlertRepository())
            {
                IList<DeviceAlert> alerts = await deviceAlertRepository.GetDeviceAlerts(paginationFilter.PageNumber, paginationFilter.PageSize, serialNumber, status);
                IList<AlertResponse> alertResponses = new List<AlertResponse>();

                foreach (DeviceAlert alert in alerts)
                {
                    alertResponses.Add(new AlertResponse(
                        alert.DeviceAlertId,
                        alert.AlertStatus,
                        alert.AlertType,
                        alert.AlertDescription,
                        alert.DeviceAlertDate,
                        alert.DeviceAlertUpdateDate,
                        alert.MinReading,
                        alert.MaxReading));
                }
                int total = await deviceAlertRepository.GetAlertsAmount(serialNumber, status);
                return new PagedResponse<IList<AlertResponse>>(alertResponses, paginationFilter.PageNumber, paginationFilter.PageSize, total);
            }
        }
    }
}
