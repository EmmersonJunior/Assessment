using Theoremone.SmartAc.Api.Models;

namespace Theoremone.SmartAc.Services
{
    /// <summary>
    /// Used to create links for pages in alert response.
    /// </summary>
    public interface IUriService
    {
        /// <summary>
        /// Populuate an paged response with Links.
        /// </summary>
        /// <typeparam name="T">The generic response.</typeparam>
        /// <param name="pagedResponse">The paged response without links.</param>
        /// <param name="filter">the pagination filter input parameter.</param>
        /// <param name="route">The endpoint route.</param>
        /// <returns>Paged response with links.</returns>
        PagedResponse<T> PopulatePagedResponse<T>(PagedResponse<T> pagedResponse, PaginationFilter filter, string route);
    }
}
