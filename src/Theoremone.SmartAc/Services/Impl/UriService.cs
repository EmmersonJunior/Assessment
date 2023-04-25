using Microsoft.AspNetCore.WebUtilities;
using Theoremone.SmartAc.Api.Models;

namespace Theoremone.SmartAc.Services.Impl
{
    /// <summary>
    /// Used to create links for pages in alert response.
    /// </summary>
    public class UriService : IUriService
    {
        private const int FIRST_PAGE = 0;
        private readonly string _baseUri;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="baseUri">The base uri.</param>
        public UriService(string baseUri)
        {
            _baseUri = baseUri;
        }

        /// <summary>
        /// Populuate an paged response with Links.
        /// </summary>
        /// <typeparam name="T">The generic response.</typeparam>
        /// <param name="pagedResponse">The paged response without links.</param>
        /// <param name="filter">the pagination filter input parameter.</param>
        /// <param name="route">The endpoint route.</param>
        /// <returns>Paged response with links.</returns>
        public PagedResponse<T> PopulatePagedResponse<T>(PagedResponse<T> pagedResponse, PaginationFilter filter, string route)
        {
            SetNextPageUri(pagedResponse, filter, route);
            SetPreviousPageUri(pagedResponse, filter, route);
            SetFirstPageUri(pagedResponse, filter, route);
            SetLastPageUri(pagedResponse, filter, route);

            return pagedResponse;
        }

        private void SetNextPageUri<T>(PagedResponse<T> pagedResponse, PaginationFilter filter, string route)
        {
            if (pagedResponse.PageNumber >= pagedResponse.TotalPages - 1)
            {
                pagedResponse.NextPage = GetPageUri(route, pagedResponse.TotalPages - 1, pagedResponse.PageSize);
            }
            else
            {
                if (pagedResponse.PageNumber < FIRST_PAGE)
                {
                    pagedResponse.NextPage = GetPageUri(route, FIRST_PAGE, pagedResponse.PageSize);
                }
                else
                {
                    pagedResponse.NextPage = GetPageUri(route, (pagedResponse.PageNumber + 1), pagedResponse.PageSize);
                }
            }
        }

        private void SetPreviousPageUri<T>(PagedResponse<T> pagedResponse, PaginationFilter filter, string route)
        {
            if (pagedResponse.PageNumber <= FIRST_PAGE)
            {
                pagedResponse.PreviousPage = GetPageUri(route, FIRST_PAGE, pagedResponse.PageSize);
            }
            else
            {
                if (pagedResponse.PageNumber > pagedResponse.TotalPages - 1)
                {
                    pagedResponse.PreviousPage = GetPageUri(route, pagedResponse.TotalPages - 1, pagedResponse.PageSize);
                }
                else
                {
                    pagedResponse.PreviousPage = GetPageUri(route, (pagedResponse.PageNumber - 1), pagedResponse.PageSize);
                }
            }
        }

        private void SetFirstPageUri<T>(PagedResponse<T> pagedResponse, PaginationFilter filter, string route)
        {
            pagedResponse.FirstPage = GetPageUri(route, 0, pagedResponse.PageSize);
        }

        private void SetLastPageUri<T>(PagedResponse<T> pagedResponse, PaginationFilter filter, string route)
        {
            pagedResponse.LastPage = GetPageUri(route, pagedResponse.TotalPages - 1, pagedResponse.PageSize);
        }

        private Uri GetPageUri(string route, int pageNumber, int pageSize)
        {
            Uri _enpointUri = new Uri(string.Concat(_baseUri, route));
            string modifiedUri = QueryHelpers.AddQueryString(_enpointUri.ToString(), "pageNumber", pageNumber.ToString());
            modifiedUri = QueryHelpers.AddQueryString(modifiedUri, "pageSize", pageSize.ToString());
            return new Uri(modifiedUri);
        }
    }
}
