namespace Theoremone.SmartAc.Api.Models
{
    public class PaginationFilter
    {
        /// <summary>
        /// The requested page number.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// The page size.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// The search alert status.
        /// </summary>
        public AlertStatusSearchEnum AlertStatus { get; set; }

        /// <summary>
        /// Empty contructor.
        /// </summary>
        public PaginationFilter()
        {
            PageNumber = 1;
            PageSize = 10;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="statusSearchEnum">The status search enum.</param>
        public PaginationFilter(int pageNumber, int pageSize, AlertStatusSearchEnum statusSearchEnum)
        {
            PageNumber = pageNumber < 0 ? 0 : pageNumber;
            AlertStatus = statusSearchEnum;

            // TODO: retrieve this prop from property file
            if (pageSize > 50)
            {
                PageSize = 50;
            } 
            else if (pageSize < 1)
            {
                PageSize = 1;
            }
            else
            {
                PageSize = pageSize;
            }
        }
    }
}
