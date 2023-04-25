using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Theoremone.SmartAc.Api.Bindings;
using Theoremone.SmartAc.Api.Models;
using Theoremone.SmartAc.Services;

namespace Theoremone.SmartAc.Api.Controllers;

[ApiController]
[Route("api/v1/device")]
[Authorize("DeviceIngestion")]
public class DeviceAlertsController : ControllerBase
{
    private readonly ILogger<DeviceIngestionController> _logger;
    private readonly IDeviceAlertProcessingService _deviceAlertProcessingService;
    private readonly IUriService _uriService;

    public DeviceAlertsController(
        ILogger<DeviceIngestionController> logger,
        IDeviceAlertProcessingService deviceAlertProcessingService,
        IUriService uriService)
    {
        _logger = logger;
        _deviceAlertProcessingService = deviceAlertProcessingService;
        _uriService = uriService;
    }

    /// <summary>
    /// Allow a device to read its alerts.
    /// </summary>
    /// <param name="serialNumber">The device serial number.</param>
    /// <param name="pageSize">The page size of a request.</param>
    /// <param name="pageNumber">The page number of a request.</param>
    /// <param name="status">The possible status for search.</param>
    /// <response code="401">If jwt token provided is invalid.</response>
    /// <response code="200">If sensor alerts are retrieved successfully.</response>
    /// <returns></returns>
    [HttpGet("alerts", Name = "GetAlerts")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<AlertResponse>>> GetAlerts(
        [ModelBinder(BinderType = typeof(DeviceInfoBinder))] string serialNumber,
        [FromQuery] int pageSize,
        [FromQuery] int pageNumber,
        [FromQuery] AlertStatusSearchEnum status)
    {
        try
        {
            PaginationFilter filter = new PaginationFilter(pageNumber, pageSize, status);
            PagedResponse<IList<AlertResponse>> pagedResponse = await _deviceAlertProcessingService.GetAlerts(serialNumber, filter, status);
            pagedResponse = _uriService.PopulatePagedResponse(pagedResponse, filter, Url.RouteUrl("GetAlerts"));
            return Ok(pagedResponse);
        }
        catch (Exception)
        {
            return Problem(
                detail: "Internal Server Error.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}