using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Theoremone.SmartAc.Data;
using Theoremone.SmartAc.Data.Models;
using Theoremone.SmartAc.Repository;
using Theoremone.SmartAc.Services.Impl;

namespace Theoremone.SmartAc.Test;

public class DeviceIAlertProcessingServiceTest
{
    private IConfiguration _configuration;
    private readonly Mock<IDeviceAlertRepository> _deviceAlertRepository;
    private readonly Mock<ILogger<DeviceAlertProcessingService>> _logger;
    private readonly Mock<SmartAcContext> _context;

    private readonly DeviceAlertProcessingService _deviceAlertProcessingService;

    public DeviceIAlertProcessingServiceTest()
    {
        _logger = new Mock<ILogger<DeviceAlertProcessingService>>();
        _deviceAlertRepository = new Mock<IDeviceAlertRepository>();
        _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                 .AddEnvironmentVariables()
                 .Build();
        _context = new Mock<SmartAcContext>();

        _deviceAlertProcessingService = new DeviceAlertProcessingService(_configuration, _logger.Object);
    }

    [Theory]
    [InlineData(0.99)]
    [InlineData(5.01)]
    [InlineData(6)]
    [InlineData(0.10)]
    public void CheckSensors_WithCoOutOfRange_ShouldCreateSprecializedAlert(decimal coReading)
    {
        // Arrange
        DeviceReading deviceReading = new DeviceReading()
        {
            CarbonMonoxide = coReading,
            DeviceReadingId = 1,
            DeviceSerialNumber = Guid.NewGuid().ToString(),
            Health = DeviceHealth.Ok,
            Humidity = 20,
            Temperature = 30,
        };

        // Act
        IList<DeviceAlert> deviceAlert = _deviceAlertProcessingService.CheckSensors(deviceReading);

        // Assert
        Assert.NotNull(deviceAlert.Where(dev => dev.AlertType.Equals(AlertTypeEnum.CO_OUT_OF_RANGE)));
    }

    [Theory]
    [InlineData(9.99)]
    [InlineData(10.01)]
    [InlineData(11)]
    [InlineData(6.10)]
    public void CheckSensors_WithTempOutOfRange_ShouldCreateSprecializedAlert(decimal tempReading)
    {
        // Arrange
        DeviceReading deviceReading = new DeviceReading()
        {
            CarbonMonoxide = 2,
            DeviceReadingId = 1,
            DeviceSerialNumber = Guid.NewGuid().ToString(),
            Health = DeviceHealth.Ok,
            Humidity = 20,
            Temperature = tempReading,
        };

        // Act
        IList<DeviceAlert> deviceAlert = _deviceAlertProcessingService.CheckSensors(deviceReading);

        // Assert
        Assert.NotNull(deviceAlert.Where(dev => dev.AlertType.Equals(AlertTypeEnum.TEMP_OUT_OF_RANGE)));
    }

    [Theory]
    [InlineData(9.99)]
    [InlineData(40.01)]
    [InlineData(50)]
    [InlineData(5.10)]
    public void CheckSensors_WithHumidityOutOfRange_ShouldCreateSprecializedAlert(decimal humidityReading)
    {
        // Arrange
        DeviceReading deviceReading = new DeviceReading()
        {
            CarbonMonoxide = 2,
            DeviceReadingId = 1,
            DeviceSerialNumber = Guid.NewGuid().ToString(),
            Health = DeviceHealth.Ok,
            Humidity = humidityReading,
            Temperature = 30,
        };

        // Act
        IList<DeviceAlert> deviceAlert = _deviceAlertProcessingService.CheckSensors(deviceReading);

        // Assert
        Assert.NotNull(deviceAlert.Where(dev => dev.AlertType.Equals(AlertTypeEnum.HUMIDITY_OUT_OF_RANGE)));
    }

    [Theory]
    [InlineData(9.01)]
    [InlineData(10)]
    public void CheckSensors_WithCoInDangerousLevel_ShouldCreateSprecializedAlert(decimal coReading)
    {
        // Arrange
        DeviceReading deviceReading = new DeviceReading()
        {
            CarbonMonoxide = coReading,
            DeviceReadingId = 1,
            DeviceSerialNumber = Guid.NewGuid().ToString(),
            Health = DeviceHealth.Ok,
            Humidity = 20,
            Temperature = 30,
        };

        // Act
        IList<DeviceAlert> deviceAlert = _deviceAlertProcessingService.CheckSensors(deviceReading);

        // Assert
        Assert.NotNull(deviceAlert.Where(dev => dev.AlertType.Equals(AlertTypeEnum.DANGEROUS_CO_LEVELS)));
        Assert.NotNull(deviceAlert.Where(dev => dev.AlertType.Equals(AlertTypeEnum.CO_OUT_OF_RANGE)));
    }

    [Theory]
    [InlineData(DeviceHealth.NeedFilter)]
    [InlineData(DeviceHealth.NeedService)]
    public void CheckSensors_WithPoorHealth_ShouldCreateSprecializedAlert(DeviceHealth deviceHealth)
    {
        // Arrange
        DeviceReading deviceReading = new DeviceReading()
        {
            CarbonMonoxide = 2,
            DeviceReadingId = 1,
            DeviceSerialNumber = Guid.NewGuid().ToString(),
            Health = deviceHealth,
            Humidity = 20,
            Temperature = 30,
        };

        // Act
        IList<DeviceAlert> deviceAlert = _deviceAlertProcessingService.CheckSensors(deviceReading);

        // Assert
        Assert.NotNull(deviceAlert.Where(dev => dev.AlertType.Equals(AlertTypeEnum.POOR_HEALTH)));
    }


    [Fact]
    public void CheckSensors_WithAnyProblem_ShouldNotCreateSprecializedAlert()
    {
        // Arrange
        DeviceReading deviceReading = new DeviceReading()
        {
            CarbonMonoxide = 2,
            DeviceReadingId = 1,
            DeviceSerialNumber = Guid.NewGuid().ToString(),
            Health = DeviceHealth.Ok,
            Humidity = 20,
            Temperature = 30,
        };

        // Act
        IList<DeviceAlert> deviceAlert = _deviceAlertProcessingService.CheckSensors(deviceReading);

        // Assert
        Assert.Empty(deviceAlert);
    }
}
