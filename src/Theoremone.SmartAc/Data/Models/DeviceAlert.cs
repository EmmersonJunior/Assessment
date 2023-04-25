namespace Theoremone.SmartAc.Data.Models;

public class DeviceAlert
{
    public DeviceAlert()
    {
    }

    public DeviceAlert(AlertTypeEnum alertTypeEnum, DeviceReading deviceReading, string description, decimal readingValue)
    {
        AlertType = alertTypeEnum;
        DeviceReadingId = deviceReading.DeviceReadingId;
        AlertDescription = description;
        AlertStatus = AlertStatusEnum.NEW;
        DeviceAlertDate = deviceReading.RecordedDateTime;
        DeviceAlertUpdateDate = deviceReading.RecordedDateTime;
        AlertUpdateTimes = 1;
        MinReading = readingValue;
        MaxReading = readingValue;
    }

    public DeviceAlert(AlertTypeEnum alertTypeEnum, DeviceReading deviceReading, string description)
    {
        AlertType = alertTypeEnum;
        DeviceReadingId = deviceReading.DeviceReadingId;
        AlertDescription = description;
        AlertStatus = AlertStatusEnum.NEW;
        DeviceAlertDate = deviceReading.RecordedDateTime;
        DeviceAlertUpdateDate = deviceReading.RecordedDateTime;
        AlertUpdateTimes = 1;
    }

    public int DeviceAlertId { get; set; }

    public AlertStatusEnum AlertStatus { get; set; }

    public AlertTypeEnum AlertType { get; set; }

    public string? AlertDescription { get; set; }

    public decimal? MinReading { get; set; }

    public decimal? MaxReading { get; set; }

    public DateTimeOffset? DeviceAlertDate { get; set; }

    public DateTimeOffset? DeviceAlertUpdateDate { get; set; }

    public int? AlertUpdateTimes { get; set; }

    public int? DeviceReadingId { get; set; }

    public DeviceReading? DeviceReading { get; set; }
}