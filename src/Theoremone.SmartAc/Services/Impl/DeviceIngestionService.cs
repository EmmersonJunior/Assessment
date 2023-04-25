using Theoremone.SmartAc.Api.Models;
using Theoremone.SmartAc.Data.Models;
using Theoremone.SmartAc.Exceptions;
using Theoremone.SmartAc.Identity;
using Theoremone.SmartAc.Repository;

namespace Theoremone.SmartAc.Services.Impl
{
    public class DeviceIngestionService : IDeviceIngestionService
    {
        private readonly IDeviceRegistrationRepository _deviceRegistrationRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IDeviceReadingRepository _deviceReadingRepository;
        private readonly SmartAcJwtService _smartAcJwtService;
        private readonly IDeviceAlertProcessingService _deviceAlertProcessing;
        private readonly ILogger<DeviceIngestionService> _logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="deviceRegistrationRepository">Manage the queries for Device Registration entity.</param>
        /// <param name="deviceRepository">Manage the queries for Device entity.</param>
        /// <param name="deviceReadingRepository">Manage the queries for Device Reading entity.</param>
        /// <param name="smartAcJwtService">JWT Service.</param>
        /// <param name="deviceAlertProcessing">Manage the Alert Processing for readings.</param>
        /// <param name="logger">The logging component.</param>
        public DeviceIngestionService(IDeviceRegistrationRepository deviceRegistrationRepository,
            IDeviceRepository deviceRepository,
            IDeviceReadingRepository deviceReadingRepository,
            SmartAcJwtService smartAcJwtService,
            IDeviceAlertProcessingService deviceAlertProcessing,
            ILogger<DeviceIngestionService> logger)
        {
            _deviceRegistrationRepository = deviceRegistrationRepository;
            _deviceRepository = deviceRepository;
            _deviceReadingRepository = deviceReadingRepository;
            _smartAcJwtService = smartAcJwtService;
            _deviceAlertProcessing = deviceAlertProcessing;
            _logger = logger;
        }

        /// <summary>
        /// Register a new device and generates a token for it.
        /// </summary>
        /// <param name="serialNumber">The device serial number.</param>
        /// <param name="sharedSecret">The device registration secret.</param>
        /// <param name="firmwareVersion">The firmware version.</param>
        /// <returns>A new token required for future transactions.</returns>
        /// <exception cref="UnauthorizedException">If the serial number does not match with database value.</exception>
        public async Task<string?> RegisterDevice(string serialNumber, string sharedSecret, string firmwareVersion)
        {
            Device? device = await _deviceRepository.GetDeviceBySerialNumberAndSharedSecret(serialNumber, sharedSecret);

            if (device == null)
            {
                string error = $"There is not a matching device for serial number {serialNumber} and the secret provided.";
                _logger.LogDebug(error, serialNumber);
                throw new UnauthorizedException(error);
            }

            var (tokenId, jwtToken) =
                _smartAcJwtService.GenerateJwtFor(serialNumber, SmartAcJwtService.JwtScopeDeviceIngestionService);
            var newRegistrationDevice = new DeviceRegistration()
            {
                Device = device,
                TokenId = tokenId
            };

            await _deviceRegistrationRepository.DeactivateRegistratiosBySerialNumber(device.SerialNumber, commit: false);
            await _deviceRegistrationRepository.AddRegistration(newRegistrationDevice, commit: false);
            await _deviceRepository.UpdateDetails(device, firmwareVersion, newRegistrationDevice, commit: true);

            _logger.LogDebug(
                $"A new registration record with tokenId {tokenId} has been created for the device {serialNumber}",
                serialNumber, tokenId);

            return jwtToken;
        }

        /// <summary>
        /// Process the sensor readings and persist the results.
        /// </summary>
        /// <param name="serialNumber">The device serial number.</param>
        /// <param name="deviceReadingRecords">The readings from device.</param>
        /// <returns></returns>
        public async Task AddSensorReadings(string serialNumber, IEnumerable<DeviceReadingRecord> deviceReadingRecords)
        {
            var receivedDate = DateTime.UtcNow;
            var deviceReadings = deviceReadingRecords
                .OrderBy(device => device.RecordedDateTime)
                .Select(reading => reading.ToDeviceReading(serialNumber, receivedDate))
                .ToList();
            await _deviceReadingRepository.AddDevices(deviceReadings, commit: true);
            _deviceAlertProcessing.PushDeviceReadingEvent(deviceReadings.Select(device => device.DeviceReadingId));
        }
    }
}
