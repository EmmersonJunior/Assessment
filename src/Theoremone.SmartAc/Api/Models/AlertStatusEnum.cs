namespace Theoremone.SmartAc.Api.Models
{
    /// <summary>
    /// Maps the possible values for a search alert Status.
    /// </summary>
    public enum AlertStatusSearchEnum
    {
        /// <summary>
        /// Retrieve new alerts.
        /// </summary>
        NEW = 1,

        /// <summary>
        /// Retrieve resolved alerts.
        /// </summary>
        RESOLVED = 2,

        /// <summary>
        /// Retrieve all results.
        /// </summary>
        ALL = 0
    }
}
