using System.ComponentModel.DataAnnotations.Schema;

namespace Theoremone.SmartAc.Data.Models;

public class DeviceReading
{
    public int DeviceReadingId { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal Temperature { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal Humidity { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal CarbonMonoxide { get; set; }

    public DeviceHealth Health { get; set; }
    public DateTimeOffset RecordedDateTime { get; set; }
    public DateTimeOffset ReceivedDateTime { get; set; } = DateTime.UtcNow;

    public string DeviceSerialNumber { get; set; } = string.Empty;
    public Device Device { get; set; } = null!;

    public ICollection<DeviceAlert> DeviceAlerts { get; set; } = new List<DeviceAlert>();

    /// <summary>
    /// Define the lower reading.
    /// </summary>
    /// <param name="oldReading">The lower registered reading.</param>
    /// <param name="type"></param>
    /// <returns>Lower reading.</returns>
    public decimal? DefineMinReading(decimal? oldReading, AlertTypeEnum type)
    {
        switch (type)
        {
            case AlertTypeEnum.DANGEROUS_CO_LEVELS:
            case AlertTypeEnum.CO_OUT_OF_RANGE:
                return oldReading < CarbonMonoxide ? oldReading : CarbonMonoxide;
            case AlertTypeEnum.TEMP_OUT_OF_RANGE:
                return oldReading < Temperature ? oldReading : Temperature;
            case AlertTypeEnum.HUMIDITY_OUT_OF_RANGE:
                return oldReading < Humidity ? oldReading : Humidity;

            default: 
                return oldReading;
        }
    }

    /// <summary>
    /// Define the higher reading.
    /// </summary>
    /// <param name="oldReading">The higher registered reading.</param>
    /// <param name="type"></param>
    /// <returns>Higher reading.</returns>
    public decimal? DefineMaxReading(decimal? oldReading, AlertTypeEnum type)
    {
        switch (type)
        {
            case AlertTypeEnum.DANGEROUS_CO_LEVELS:
            case AlertTypeEnum.CO_OUT_OF_RANGE:
                return oldReading > CarbonMonoxide ? oldReading : CarbonMonoxide;
            case AlertTypeEnum.TEMP_OUT_OF_RANGE:
                return oldReading > Temperature ? oldReading : Temperature;
            case AlertTypeEnum.HUMIDITY_OUT_OF_RANGE:
                return oldReading > Humidity ? oldReading : Humidity;

            default:
                return oldReading;
        }
    }
}