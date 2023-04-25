using Theoremone.SmartAc.Data.Models;

namespace Theoremone.SmartAc.Api.Models;

public record DeviceReadingRecord(
    DateTimeOffset RecordedDateTime,
    decimal Temperature,
    decimal Humidity,
    decimal CarbonMonoxide,
    DeviceHealth Health)
{
    public DeviceReading ToDeviceReading(string serialNumber, DateTimeOffset receivedDate)
    {
        return new()
        {
            DeviceSerialNumber = serialNumber,
            RecordedDateTime = RecordedDateTime,
            ReceivedDateTime = receivedDate,
            Temperature = Temperature,
            Humidity = Humidity,
            CarbonMonoxide = CarbonMonoxide,
            Health = Health,
        };
    }
}