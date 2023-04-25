using System.Text.Json.Serialization;
using Theoremone.SmartAc.Data.Models;

namespace Theoremone.SmartAc.Api.Models
{
    public class AlertResponse
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="type"></param>
        /// <param name="description"></param>
        /// <param name="alertDate"></param>
        /// <param name="alertUpdateDate"></param>
        /// <param name="maxReading">The sensor max reading for alert.</param>
        /// <param name="minReading">The sensor min reading for alert.</param>
        public AlertResponse(int id, AlertStatusEnum status, AlertTypeEnum type, string? description, DateTimeOffset? alertDate, DateTimeOffset? alertUpdateDate, decimal? minReading, decimal? maxReading)
        {
            Id = id;
            Status = status;
            Type = type;
            Description = description;
            AlertDate = alertDate;
            AlertUpdateDate = alertUpdateDate;
            MinReading = minReading;
            MaxReading = maxReading;
        }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("status")]
        public AlertStatusEnum Status { get; set; }

        [JsonPropertyName("type")]
        public AlertTypeEnum Type { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("min_reading")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public decimal? MinReading { get; set; }

        [JsonPropertyName("max_reading")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public decimal? MaxReading { get; set; }

        [JsonPropertyName("alert_date")]
        public DateTimeOffset? AlertDate { get; set; }

        [JsonPropertyName("alert_last_update_date")]
        public DateTimeOffset? AlertUpdateDate { get; set; }
    }
}
