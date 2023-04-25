using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Theoremone.SmartAc.Data.Models;

namespace Theoremone.SmartAc.Data;

/// <summary>
/// Smart AC Db context targeting Sqlite provider
/// </summary>
public class SmartAcContext : DbContext
{
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<DeviceRegistration> DeviceRegistrations => Set<DeviceRegistration>();
    public DbSet<DeviceReading> DeviceReadings => Set<DeviceReading>();
    public DbSet<DeviceAlert> DeviceAlerts => Set<DeviceAlert>();

    public SmartAcContext(DbContextOptions<SmartAcContext> options) : base(options)
    {
    }

    public SmartAcContext()
    {        
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetToStringConverter>(); // SqlLite workaround for DateTimeOffset sorting

        configurationBuilder
            .Properties<decimal>()
            .HaveConversion<double>(); // SqlLite workaround for decimal aggregations
    }
}