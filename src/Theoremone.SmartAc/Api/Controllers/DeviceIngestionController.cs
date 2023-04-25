using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Theoremone.SmartAc.Api.Bindings;
using Theoremone.SmartAc.Api.Models;
using Theoremone.SmartAc.Exceptions;
using Theoremone.SmartAc.Services;

namespace Theoremone.SmartAc.Api.Controllers;

[ApiController]
[Route("api/v1/device")]
[Authorize("DeviceIngestion")]
public class DeviceIngestionController : ControllerBase
{
    private const string FIRMWARE_REGEX = "^(0|[1-9]\\d*)\\.(0|[1-9]\\d*)\\.(0|[1-9]\\d*)(?:-((?:0|[1-9]\\d*|\\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\\.(?:0|[1-9]\\d*|\\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\\+([0-9a-zA-Z-]+(?:\\.[0-9a-zA-Z-]+)*))?$";

    private readonly ILogger<DeviceIngestionController> _logger;
    private readonly IDeviceIngestionService _deviceIngestionService;

    public DeviceIngestionController(
        ILogger<DeviceIngestionController> logger,
        IDeviceIngestionService deviceIngestionService)
    {
        _logger = logger;
        _deviceIngestionService = deviceIngestionService;
    }

    /// <summary>
    /// Allow smart ac devices to register themselves  
    /// and get a jwt token for subsequent operations
    /// </summary>
    /// <param name="serialNumber">Unique device identifier burned into ROM</param>
    /// <param name="sharedSecret">Unique device shareable secret burned into ROM</param>
    /// <param name="firmwareVersion">Device firmware version at the moment of registering</param>
    /// <returns>A jwt token</returns>
    /// <response code="400">If any of the required data is not pressent or is invalid.</response>
    /// <response code="401">If something is wrong on the information provided.</response>
    /// <response code="200">If the registration has sucesfully generated a new jwt token.</response>
    [HttpPost("{serialNumber}/registration")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegisterDevice(
        [Required][FromRoute] string serialNumber,
        [Required][FromHeader(Name = "x-device-shared-secret")] string sharedSecret,
        [Required][FromQuery] string firmwareVersion)
    {
        try
        {
            bool isFirmawareVersionValid = Regex.IsMatch(firmwareVersion, FIRMWARE_REGEX);

            if (!isFirmawareVersionValid)
            {
                string error = "The firmware value does not match semantic versioning format.";
                _logger.LogInformation(error);

                ModelState.AddModelError("firmwareVersion", error);
                return ValidationProblem();
            }

            string? jwtToken = await _deviceIngestionService.RegisterDevice(serialNumber, sharedSecret, firmwareVersion);

            return Ok(jwtToken);
        }
        catch (UnauthorizedException)
        {
            return Problem(
                detail: "Something is wrong on the information provided, please review.",
                statusCode: StatusCodes.Status401Unauthorized);
        }
    }

    /// <summary>
    /// Allow smart ac devices to send sensor readings in batch
    /// 
    /// This will additionally trigger analysis over the sensor readings
    /// to generate alerts based on it
    /// </summary>
    /// <param name="serialNumber">Unique device identifier burned into ROM.</param>
    /// <param name="sensorReadings">Collection of sensor readings send by a device.</param>
    /// <response code="401">If jwt token provided is invalid.</response>
    /// <response code="202">If sensor readings has sucesfully accepted.</response>
    /// <returns>No Content.</returns>
    [HttpPost("readings/batch")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> AddSensorReadings(
        [ModelBinder(BinderType = typeof(DeviceInfoBinder))] string serialNumber,
        [FromBody] IEnumerable<DeviceReadingRecord> sensorReadings)
    {
        try
        {
            await _deviceIngestionService.AddSensorReadings(serialNumber, sensorReadings);
            return Accepted();
        }
        catch (Exception)
        {
            return Problem(
                detail: "Internal Server Error.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}