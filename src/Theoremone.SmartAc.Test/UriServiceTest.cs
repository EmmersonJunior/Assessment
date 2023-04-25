using Theoremone.SmartAc.Api.Models;
using Theoremone.SmartAc.Services.Impl;

namespace Theoremone.SmartAc.Test;

public class UriServiceTest
{
    [Fact]
    public void PopulatePagedResponse_OnPageZero_ShouldCreateLinks()
    {
        // Arrange
        string data = "fakeData";
        int pageNumber = 0;
        int pageSize = 10;
        int totalRecords = 30;
        string route = "api/test";
        string host = "http://localhost/";

        UriService service = new UriService(host);
        PagedResponse<string> pagedResponse = new PagedResponse<string>(data, pageNumber, pageSize, totalRecords);
        PaginationFilter filter = new PaginationFilter();

        // Act
        PagedResponse<string> filledPagedResponse = service.PopulatePagedResponse(pagedResponse, filter, route);

        // Assert
        Assert.Equal(string.Concat(host, route, $"?pageNumber={1}&pageSize={pageSize}"), filledPagedResponse.NextPage.ToString());
        Assert.Equal(string.Concat(host, route, $"?pageNumber={2}&pageSize={pageSize}"), filledPagedResponse.LastPage.ToString());
        Assert.Equal(string.Concat(host, route, $"?pageNumber={0}&pageSize={pageSize}"), filledPagedResponse.PreviousPage.ToString());
        Assert.Equal(string.Concat(host, route, $"?pageNumber={0}&pageSize={pageSize}"), filledPagedResponse.FirstPage.ToString());
    }

    [Fact]
    public void PopulatePagedResponse_OnPageZeroAndNoMorePages_ShouldCreateLinks()
    {
        // Arrange
        string data = "fakeData";
        int pageNumber = 0;
        int pageSize = 10;
        int totalRecords = 9;
        string route = "api/test";
        string host = "http://localhost/";

        UriService service = new UriService(host);
        PagedResponse<string> pagedResponse = new PagedResponse<string>(data, pageNumber, pageSize, totalRecords);
        PaginationFilter filter = new PaginationFilter();

        // Act
        PagedResponse<string> filledPagedResponse = service.PopulatePagedResponse(pagedResponse, filter, route);

        // Assert
        Assert.Equal(string.Concat(host, route, $"?pageNumber={0}&pageSize={pageSize}"), filledPagedResponse.NextPage.ToString());
        Assert.Equal(string.Concat(host, route, $"?pageNumber={0}&pageSize={pageSize}"), filledPagedResponse.LastPage.ToString());
        Assert.Equal(string.Concat(host, route, $"?pageNumber={0}&pageSize={pageSize}"), filledPagedResponse.PreviousPage.ToString());
        Assert.Equal(string.Concat(host, route, $"?pageNumber={0}&pageSize={pageSize}"), filledPagedResponse.FirstPage.ToString());
    }

    [Fact]
    public void PopulatePagedResponse_OnPageTwoWithMorePages_ShouldCreateLinks()
    {
        // Arrange
        string data = "fakeData";
        int pageNumber = 1;
        int pageSize = 5;
        int totalRecords = 12;
        string route = "api/test";
        string host = "http://localhost/";

        UriService service = new UriService(host);
        PagedResponse<string> pagedResponse = new PagedResponse<string>(data, pageNumber, pageSize, totalRecords);
        PaginationFilter filter = new PaginationFilter();

        // Act
        PagedResponse<string> filledPagedResponse = service.PopulatePagedResponse(pagedResponse, filter, route);

        // Assert
        Assert.Equal(string.Concat(host, route, $"?pageNumber={2}&pageSize={pageSize}"), filledPagedResponse.NextPage.ToString());
        Assert.Equal(string.Concat(host, route, $"?pageNumber={2}&pageSize={pageSize}"), filledPagedResponse.LastPage.ToString());
        Assert.Equal(string.Concat(host, route, $"?pageNumber={0}&pageSize={pageSize}"), filledPagedResponse.PreviousPage.ToString());
        Assert.Equal(string.Concat(host, route, $"?pageNumber={0}&pageSize={pageSize}"), filledPagedResponse.FirstPage.ToString());
    }

    [Fact]
    public void PopulatePagedResponse_OnNegativePage_ShouldCreateLinks()
    {
        // Arrange
        string data = "fakeData";
        int pageNumber = -1;
        int pageSize = 5;
        int totalRecords = 12;
        string route = "api/test";
        string host = "http://localhost/";

        UriService service = new UriService(host);
        PagedResponse<string> pagedResponse = new PagedResponse<string>(data, pageNumber, pageSize, totalRecords);
        PaginationFilter filter = new PaginationFilter();

        // Act
        PagedResponse<string> filledPagedResponse = service.PopulatePagedResponse(pagedResponse, filter, route);

        // Assert
        Assert.Equal(string.Concat(host, route, $"?pageNumber={0}&pageSize={pageSize}"), filledPagedResponse.NextPage.ToString());
        Assert.Equal(string.Concat(host, route, $"?pageNumber={2}&pageSize={pageSize}"), filledPagedResponse.LastPage.ToString());
        Assert.Equal(string.Concat(host, route, $"?pageNumber={0}&pageSize={pageSize}"), filledPagedResponse.PreviousPage.ToString());
        Assert.Equal(string.Concat(host, route, $"?pageNumber={0}&pageSize={pageSize}"), filledPagedResponse.FirstPage.ToString());
    }

    [Fact]
    public void PopulatePagedResponse_OnPageNumberBiggerThanRecords_ShouldCreateLinks()
    {
        // Arrange
        string data = "fakeData";
        int pageNumber = 4;
        int pageSize = 5;
        int totalRecords = 12;
        string route = "api/test";
        string host = "http://localhost/";

        UriService service = new UriService(host);
        PagedResponse<string> pagedResponse = new PagedResponse<string>(data, pageNumber, pageSize, totalRecords);
        PaginationFilter filter = new PaginationFilter();

        // Act
        PagedResponse<string> filledPagedResponse = service.PopulatePagedResponse(pagedResponse, filter, route);

        // Assert
        Assert.Equal(string.Concat(host, route, $"?pageNumber={2}&pageSize={pageSize}"), filledPagedResponse.NextPage.ToString());
        Assert.Equal(string.Concat(host, route, $"?pageNumber={2}&pageSize={pageSize}"), filledPagedResponse.LastPage.ToString());
        Assert.Equal(string.Concat(host, route, $"?pageNumber={2}&pageSize={pageSize}"), filledPagedResponse.PreviousPage.ToString());
        Assert.Equal(string.Concat(host, route, $"?pageNumber={0}&pageSize={pageSize}"), filledPagedResponse.FirstPage.ToString());
    }
}
