using System.Text.Json.Serialization;

namespace Theoremone.SmartAc.Api.Models
{
    /// <summary>
    /// Maps a paged response.
    /// </summary>
    /// <typeparam name="T">Wrapped data response.</typeparam>
    public class PagedResponse<T> : Response<T>
    {
        /// <summary>
        /// The actual page number.
        /// </summary>
        [JsonPropertyName("page_number")]
        public int PageNumber { get; set; }

        /// <summary>
        /// The request page size.
        /// </summary>
        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }

        /// <summary>
        /// Link to first page.
        /// </summary>
        [JsonPropertyName("first_page")]
        public Uri FirstPage { get; set; }

        /// <summary>
        /// Link to last page.
        /// </summary>
        [JsonPropertyName("last_page")]
        public Uri LastPage { get; set; }

        /// <summary>
        /// Link to next page.
        /// </summary>
        [JsonPropertyName("next_page")]
        public Uri NextPage { get; set; }

        /// <summary>
        /// Link to previous page.
        /// </summary>
        [JsonPropertyName("previous_page")]
        public Uri PreviousPage { get; set; }

        /// <summary>
        /// Total amount of pages.
        /// </summary>
        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }

        /// <summary>
        /// Total amount of records on database.
        /// </summary>
        [JsonPropertyName("total_records")]
        public int TotalRecords { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="data">The wrapped response data.</param>
        /// <param name="pageNumber">The requested page number.</param>
        /// <param name="pageSize">The size of page for response.</param>
        /// <param name="totalRecords">Total amount of records on database.</param>
        public PagedResponse(T data, int pageNumber, int pageSize, int totalRecords)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            Data = data;
            TotalRecords = totalRecords;
            TotalPages = totalRecords % pageSize > 0 ? (totalRecords / pageSize) + 1 : totalRecords / pageSize;
        }
    }
}
