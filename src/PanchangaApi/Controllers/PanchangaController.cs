using Microsoft.AspNetCore.Mvc;
using PanchangaApi.Core.Models;
using PanchangaApi.Core.Services;

namespace PanchangaApi.Controllers;

/// <summary>
/// Controller for Panchanga calculations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PanchangaController : ControllerBase
{
    private readonly IPanchangaService _panchangaService;
    private readonly ILogger<PanchangaController> _logger;

    public PanchangaController(IPanchangaService panchangaService, ILogger<PanchangaController> logger)
    {
        _panchangaService = panchangaService;
        _logger = logger;
    }

    /// <summary>
    /// Get complete Panchanga information for a specific date and location
    /// </summary>
    /// <param name="year">Year (e.g., 2024)</param>
    /// <param name="month">Month (1-12)</param>
    /// <param name="day">Day (1-31)</param>
    /// <param name="latitude">Latitude in degrees (-90 to 90)</param>
    /// <param name="longitude">Longitude in degrees (-180 to 180)</param>
    /// <param name="timezone">Timezone offset in hours (-12 to 14)</param>
    /// <param name="locationName">Optional location name</param>
    /// <returns>Complete Panchanga data</returns>
    [HttpGet]
    public async Task<ActionResult<PanchangaData>> GetPanchanga(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] int day,
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double timezone,
        [FromQuery] string? locationName = null)
    {
        try
        {
            var date = new PanchangaDate(year, month, day);
            var location = new Location(latitude, longitude, timezone, locationName);

            if (!date.IsValid)
            {
                return BadRequest($"Invalid date: {year}-{month}-{day}");
            }

            if (!location.IsValid)
            {
                return BadRequest("Invalid location coordinates or timezone");
            }

            _logger.LogInformation("Calculating Panchanga for {Date} at {Location}", date, location);

            var result = await _panchangaService.CalculatePanchangaAsync(date, location);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating Panchanga");
            return StatusCode(500, "An error occurred while calculating Panchanga");
        }
    }

    /// <summary>
    /// Get Panchanga information using a simplified query
    /// </summary>
    /// <param name="request">Panchanga request</param>
    /// <returns>Complete Panchanga data</returns>
    [HttpPost]
    public async Task<ActionResult<PanchangaData>> GetPanchanga([FromBody] PanchangaRequest request)
    {
        try
        {
            if (!request.Date.IsValid)
            {
                return BadRequest("Invalid date provided");
            }

            if (!request.Location.IsValid)
            {
                return BadRequest("Invalid location provided");
            }

            _logger.LogInformation("Calculating Panchanga for {Date} at {Location}", request.Date, request.Location);

            var result = await _panchangaService.CalculatePanchangaAsync(request.Date, request.Location);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating Panchanga");
            return StatusCode(500, "An error occurred while calculating Panchanga");
        }
    }
}

/// <summary>
/// Request model for Panchanga calculation
/// </summary>
public record PanchangaRequest(
    PanchangaDate Date,
    Location Location);

/// <summary>
/// Controller for individual Panchanga elements
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ElementsController : ControllerBase
{
    private readonly IPanchangaService _panchangaService;
    private readonly IAstronomicalService _astronomicalService;
    private readonly ILogger<ElementsController> _logger;

    public ElementsController(
        IPanchangaService panchangaService,
        IAstronomicalService astronomicalService,
        ILogger<ElementsController> logger)
    {
        _panchangaService = panchangaService;
        _astronomicalService = astronomicalService;
        _logger = logger;
    }

    /// <summary>
    /// Get Tithi information for a specific date and location
    /// </summary>
    [HttpGet("tithi")]
    public async Task<ActionResult<TithiInfo>> GetTithi(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] int day,
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double timezone)
    {
        try
        {
            var date = new PanchangaDate(year, month, day);
            var location = new Location(latitude, longitude, timezone);

            if (!date.IsValid || !location.IsValid)
            {
                return BadRequest("Invalid date or location");
            }

            var julianDay = _astronomicalService.GregorianToJulian(date);
            var result = await _panchangaService.CalculateTithiAsync(julianDay, location);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating Tithi");
            return StatusCode(500, "An error occurred while calculating Tithi");
        }
    }

    /// <summary>
    /// Get Nakshatra information for a specific date and location
    /// </summary>
    [HttpGet("nakshatra")]
    public async Task<ActionResult<NakshatraInfo>> GetNakshatra(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] int day,
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double timezone)
    {
        try
        {
            var date = new PanchangaDate(year, month, day);
            var location = new Location(latitude, longitude, timezone);

            if (!date.IsValid || !location.IsValid)
            {
                return BadRequest("Invalid date or location");
            }

            var julianDay = _astronomicalService.GregorianToJulian(date);
            var result = await _panchangaService.CalculateNakshatraAsync(julianDay, location);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating Nakshatra");
            return StatusCode(500, "An error occurred while calculating Nakshatra");
        }
    }

    /// <summary>
    /// Get Yoga information for a specific date and location
    /// </summary>
    [HttpGet("yoga")]
    public async Task<ActionResult<YogaInfo>> GetYoga(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] int day,
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double timezone)
    {
        try
        {
            var date = new PanchangaDate(year, month, day);
            var location = new Location(latitude, longitude, timezone);

            if (!date.IsValid || !location.IsValid)
            {
                return BadRequest("Invalid date or location");
            }

            var julianDay = _astronomicalService.GregorianToJulian(date);
            var result = await _panchangaService.CalculateYogaAsync(julianDay, location);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating Yoga");
            return StatusCode(500, "An error occurred while calculating Yoga");
        }
    }

    /// <summary>
    /// Get Karana information for a specific date and location
    /// </summary>
    [HttpGet("karana")]
    public async Task<ActionResult<KaranaInfo>> GetKarana(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] int day,
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double timezone)
    {
        try
        {
            var date = new PanchangaDate(year, month, day);
            var location = new Location(latitude, longitude, timezone);

            if (!date.IsValid || !location.IsValid)
            {
                return BadRequest("Invalid date or location");
            }

            var julianDay = _astronomicalService.GregorianToJulian(date);
            var result = await _panchangaService.CalculateKaranaAsync(julianDay, location);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating Karana");
            return StatusCode(500, "An error occurred while calculating Karana");
        }
    }

    /// <summary>
    /// Get Vara (weekday) information for a specific date
    /// </summary>
    [HttpGet("vara")]
    public async Task<ActionResult<VaraInfo>> GetVara(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] int day)
    {
        try
        {
            var date = new PanchangaDate(year, month, day);

            if (!date.IsValid)
            {
                return BadRequest("Invalid date");
            }

            var julianDay = _astronomicalService.GregorianToJulian(date);
            var result = await _panchangaService.CalculateVaraAsync(julianDay);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating Vara");
            return StatusCode(500, "An error occurred while calculating Vara");
        }
    }
}
