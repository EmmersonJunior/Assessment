namespace Theoremone.SmartAc.Data.Models
{
    /// <summary>
    /// Maps the possible types of alert.
    /// </summary>
    public enum AlertTypeEnum
    {
        /// <summary>
        /// Temperature out of range.
        /// </summary>
        TEMP_OUT_OF_RANGE,

        /// <summary>
        /// Humidity out of range.
        /// </summary>
        HUMIDITY_OUT_OF_RANGE,

        /// <summary>
        /// Carbon Monoxide out of range.
        /// </summary>
        CO_OUT_OF_RANGE,

        /// <summary>
        /// Carbon Monoxide in a critical level.
        /// </summary>
        DANGEROUS_CO_LEVELS,

        /// <summary>
        /// Device reported poor health.
        /// </summary>
        POOR_HEALTH
    }
}
