using System.Text.Json.Serialization;

namespace Theoremone.SmartAc.Api.Models
{
    /// <summary>
    /// Hold information for a resopnse.
    /// </summary>
    public class Response<T>
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public Response()
        {            
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="data">The wrapped response data.</param>
        public Response(T data)
        {
            Data = data;
        }

        /// <summary>
        /// The wrapped response data.
        /// </summary>
        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}
