using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Theoremone.SmartAc.Api.Models;
using Theoremone.SmartAc.Data.Models;
using Theoremone.SmartAc.Exceptions;
using Theoremone.SmartAc.Identity;
using Theoremone.SmartAc.Repository;
using Theoremone.SmartAc.Services;
using Theoremone.SmartAc.Services.Impl;

namespace Theoremone.SmartAc.Test;

public class DeviceIngestionServiceTest
{
    private readonly Mock<IDeviceRegistrationRepository> _deviceRegistrationRepository;
    private readonly Mock<IDeviceRepository> _deviceRepository;
    private readonly Mock<IDeviceReadingRepository> _deviceReadingRepository;
    private readonly SmartAcJwtService _smartAcJwtService;
    private readonly Mock<IDeviceAlertProcessingService> _deviceAlertProcessingService;
    private readonly Mock<ILogger<DeviceIngestionService>> _logger;

    private readonly DeviceIngestionService _deviceIngestionService;

    public DeviceIngestionServiceTest()
    {
        _deviceRegistrationRepository = new Mock<IDeviceRegistrationRepository>();
        _deviceRepository = new Mock<IDeviceRepository>();
        _deviceReadingRepository = new Mock<IDeviceReadingRepository>();
        _deviceAlertProcessingService = new Mock<IDeviceAlertProcessingService>();
        _logger = new Mock<ILogger<DeviceIngestionService>>();

        var config = new Dictionary<string, string> {
            {"Jwt:Key", "23SDFKLJALLAKDSJYIUAYD1G89RYBV7FTASDF58GHN76ASDF98"},
            {"Jwt:Issuer", "issuer"},
            {"Jwt:Audience", "audience"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(config)
            .Build();
        _smartAcJwtService = new SmartAcJwtService(configuration);

        _deviceIngestionService = new DeviceIngestionService(_deviceRegistrationRepository.Object, _deviceRepository.Object, _deviceReadingRepository.Object, _smartAcJwtService, _deviceAlertProcessingService.Object, _logger.Object);
    }

    [Fact]
    public async Task RegisterDevice_WithNonMatchingDevice_ShouldThrowEception()
    {
        // Arrange 
        string serialNumber = Guid.NewGuid().ToString();
        string sharedSecret = Guid.NewGuid().ToString();
        string firmwareVersion = Guid.NewGuid().ToString();
        _deviceRepository.Setup(repo => repo.GetDeviceBySerialNumberAndSharedSecret(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult<Device?>(null));

        // Act
        UnauthorizedException ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _deviceIngestionService.RegisterDevice(serialNumber, sharedSecret, firmwareVersion));


        // Assert
        Assert.Equal($"There is not a matching device for serial number {serialNumber} and the secret provided.", ex.Message);
    }

    [Fact]
    public async Task RegisterDevice_WithMatchingDevice_ShouldReturnaToken()
    {
        // Arrange 
        string? serialNumber = Guid.NewGuid().ToString();
        string sharedSecret = Guid.NewGuid().ToString();
        string firmwareVersion = Guid.NewGuid().ToString();

        string tokenId = Guid.NewGuid().ToString();
        string jwtToken = Guid.NewGuid().ToString();

        Device device = new Device()
        {
            FirmwareVersion = firmwareVersion,
            FirstRegistrationDate = DateTime.UtcNow,
            LastRegistrationDate = DateTime.UtcNow,
            SerialNumber = serialNumber,
            SharedSecret = sharedSecret
        };
        _deviceRepository.Setup(repo => repo.GetDeviceBySerialNumberAndSharedSecret(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult<Device?>(device));

        // Act
        string? jwtTokenResponse = await _deviceIngestionService.RegisterDevice(serialNumber, sharedSecret, firmwareVersion);


        // Assert
        Assert.NotNull(jwtTokenResponse);
        _deviceRegistrationRepository.Verify(repo => repo.DeactivateRegistratiosBySerialNumber(It.Is<string>(x => x.Equals(serialNumber)), It.IsAny<bool>()), Times.Once());
        _deviceRegistrationRepository.Verify(repo => repo.AddRegistration(It.Is<DeviceRegistration>(x => x.Device.SerialNumber.Equals(serialNumber)), It.IsAny<bool>()), Times.Once());
        _deviceRepository.Verify(repo => repo.UpdateDetails(
            It.Is<Device>(x => x.SerialNumber.Equals(serialNumber)),
            It.Is<string>(x => x.Equals(firmwareVersion)),
            It.Is<DeviceRegistration>(x => x.Device.SerialNumber.Equals(serialNumber)),
            It.IsAny<bool>()), Times.Once());
    }

    [Fact]
    public async Task AddSensorReadings_ShouldAddDevicesAndPublisheMessagesToTheQueue()
    {
        // Arrange 
        string? serialNumber = Guid.NewGuid().ToString();

        IEnumerable<DeviceReadingRecord> deviceReadingRecords = new List<DeviceReadingRecord>()
        {
            new(DateTimeOffset.UtcNow.AddMinutes(-40), 25.0m, 73.99m, 3.22m, DeviceHealth.Ok),
            new(DateTimeOffset.UtcNow.AddMinutes(-30), 25.19m, 73.98m, 3.22m, DeviceHealth.NeedService),
            new(DateTimeOffset.UtcNow.AddMinutes(-20), 25.2m, 73.99m, 3.21m, DeviceHealth.Ok),
            new(DateTimeOffset.UtcNow.AddMinutes(-10), 25.0m, 74.0m, 3.22m, DeviceHealth.NeedFilter),
        };

        // Act
        await _deviceIngestionService.AddSensorReadings(serialNumber, deviceReadingRecords);

        // Assert
        _deviceReadingRepository.Verify(repo => repo.AddDevices(It.IsAny<List<DeviceReading>>(), It.IsAny<bool>()), Times.Once);
        _deviceAlertProcessingService.Verify(alert => alert.PushDeviceReadingEvent(It.IsAny<IEnumerable<int>>()), Times.Once);
    }
}