using Microsoft.Extensions.Logging;
using PanchangaApi.Core.Models;

namespace PanchangaApi.Core.Services;

/// <summary>
/// Service for calculating Panchanga elements based on astronomical data
/// </summary>
public interface IPanchangaService
{
    Task<PanchangaData> CalculatePanchangaAsync(PanchangaDate date, Location location);
    Task<TithiInfo> CalculateTithiAsync(double julianDay, Location location);
    Task<NakshatraInfo> CalculateNakshatraAsync(double julianDay, Location location);
    Task<YogaInfo> CalculateYogaAsync(double julianDay, Location location);
    Task<KaranaInfo> CalculateKaranaAsync(double julianDay, Location location);
    Task<VaraInfo> CalculateVaraAsync(double julianDay);
    Task<MasaInfo> CalculateMasaAsync(double julianDay, Location location);
    Task<SamvatsaraInfo> CalculateSamvatsaraAsync(double julianDay, int masaNumber);
    Task<RituInfo> CalculateRituAsync(int masaNumber);
}

public class PanchangaService : IPanchangaService
{
    private readonly IAstronomicalService _astronomicalService;
    private readonly ISanskritNamesService _sanskritNamesService;
    private readonly ILogger<PanchangaService> _logger;

    // Kali Yuga epoch in Julian Days
    private const double KALI_YUGA_EPOCH = 588465.5;

    public PanchangaService(
        IAstronomicalService astronomicalService,
        ISanskritNamesService sanskritNamesService,
        ILogger<PanchangaService> logger)
    {
        _astronomicalService = astronomicalService;
        _sanskritNamesService = sanskritNamesService;
        _logger = logger;
    }

    public async Task<PanchangaData> CalculatePanchangaAsync(PanchangaDate date, Location location)
    {
        if (!date.IsValid || !location.IsValid)
        {
            throw new ArgumentException("Invalid date or location provided");
        }

        try
        {
            var julianDay = _astronomicalService.GregorianToJulian(date);
            
            // Calculate all panchanga elements
            var tithi = await CalculateTithiAsync(julianDay, location);
            var nakshatra = await CalculateNakshatraAsync(julianDay, location);
            var yoga = await CalculateYogaAsync(julianDay, location);
            var karana = await CalculateKaranaAsync(julianDay, location);
            var vara = await CalculateVaraAsync(julianDay);
            var masa = await CalculateMasaAsync(julianDay, location);
            var samvatsara = await CalculateSamvatsaraAsync(julianDay, masa.Number);
            var ritu = await CalculateRituAsync(masa.Number);

            // Calculate sunrise and sunset
            var (_, sunrise) = _astronomicalService.Sunrise(julianDay, location);
            var (sunsetJd, sunset) = _astronomicalService.Sunset(julianDay, location);
            
            // Calculate moonrise and moonset
            var moonrise = _astronomicalService.Moonrise(julianDay, location);
            var moonset = _astronomicalService.Moonset(julianDay, location);

            // Calculate day duration
            var dayDuration = sunset.ToDecimalHours() - sunrise.ToDecimalHours();
            if (dayDuration < 0) dayDuration += 24;

            return new PanchangaData(
                date, location, tithi, nakshatra, yoga, karana, vara,
                masa, samvatsara, ritu, sunrise, sunset, moonrise, moonset, dayDuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating Panchanga for date {Date} at location {Location}", date, location);
            throw;
        }
    }

    public async Task<TithiInfo> CalculateTithiAsync(double julianDay, Location location)
    {
        await Task.CompletedTask; // For async pattern
        
        var (sunriseJd, _) = _astronomicalService.Sunrise(julianDay, location);
        var moonPhase = _astronomicalService.LunarPhase(sunriseJd);
        
        var tithiNumber = (int)Math.Ceiling(moonPhase / 12.0);
        if (tithiNumber == 0) tithiNumber = 30;
        
        var tithiName = _sanskritNamesService.GetTithiName(tithiNumber);
        
        // Calculate end time (simplified)
        var degreesLeft = tithiNumber * 12 - moonPhase;
        var endTime = CalculateEndTime(sunriseJd, degreesLeft, 12.0, location);
        
        return new TithiInfo(tithiNumber, tithiName, endTime);
    }

    public async Task<NakshatraInfo> CalculateNakshatraAsync(double julianDay, Location location)
    {
        await Task.CompletedTask; // For async pattern
        
        var (sunriseJd, _) = _astronomicalService.Sunrise(julianDay, location);
        var lunarLongitude = _astronomicalService.LunarLongitude(sunriseJd);
        var ayanamsa = _astronomicalService.GetAyanamsa(sunriseJd);
        
        // Convert to sidereal longitude
        var siderealLongitude = (lunarLongitude - ayanamsa + 360) % 360;
        
        var nakshatraNumber = (int)Math.Ceiling(siderealLongitude * 27 / 360);
        if (nakshatraNumber == 0) nakshatraNumber = 27;
        
        var nakshatraName = _sanskritNamesService.GetNakshatraName(nakshatraNumber);
        
        // Calculate end time
        var degreesLeft = nakshatraNumber * (360.0 / 27) - siderealLongitude;
        var endTime = CalculateEndTime(sunriseJd, degreesLeft, 360.0 / 27, location);
        
        return new NakshatraInfo(nakshatraNumber, nakshatraName, endTime);
    }

    public async Task<YogaInfo> CalculateYogaAsync(double julianDay, Location location)
    {
        await Task.CompletedTask; // For async pattern
        
        var (sunriseJd, _) = _astronomicalService.Sunrise(julianDay, location);
        var lunarLongitude = _astronomicalService.LunarLongitude(sunriseJd);
        var solarLongitude = _astronomicalService.SolarLongitude(sunriseJd);
        var ayanamsa = _astronomicalService.GetAyanamsa(sunriseJd);
        
        // Convert to sidereal longitudes
        var siderealLunar = (lunarLongitude - ayanamsa + 360) % 360;
        var siderealSolar = (solarLongitude - ayanamsa + 360) % 360;
        
        var total = (siderealLunar + siderealSolar) % 360;
        
        var yogaNumber = (int)Math.Ceiling(total * 27 / 360);
        if (yogaNumber == 0) yogaNumber = 27;
        
        var yogaName = _sanskritNamesService.GetYogaName(yogaNumber);
        
        // Calculate end time
        var degreesLeft = yogaNumber * (360.0 / 27) - total;
        var endTime = CalculateEndTime(sunriseJd, degreesLeft, 360.0 / 27, location);
        
        return new YogaInfo(yogaNumber, yogaName, endTime);
    }

    public async Task<KaranaInfo> CalculateKaranaAsync(double julianDay, Location location)
    {
        await Task.CompletedTask; // For async pattern
        
        var (sunriseJd, _) = _astronomicalService.Sunrise(julianDay, location);
        var moonPhase = _astronomicalService.LunarPhase(sunriseJd);
        
        var karanaNumber = (int)Math.Ceiling(moonPhase / 6.0);
        if (karanaNumber == 0) karanaNumber = 60;
        
        var karanaName = _sanskritNamesService.GetKaranaName(karanaNumber);
        
        return new KaranaInfo(karanaNumber, karanaName);
    }

    public async Task<VaraInfo> CalculateVaraAsync(double julianDay)
    {
        await Task.CompletedTask; // For async pattern
        
        var varaNumber = (int)((julianDay + 1) % 7);
        var varaName = _sanskritNamesService.GetVaraName(varaNumber);
        
        return new VaraInfo(varaNumber, varaName);
    }

    public async Task<MasaInfo> CalculateMasaAsync(double julianDay, Location location)
    {
        await Task.CompletedTask; // For async pattern
        
        var solarLongitude = _astronomicalService.SolarLongitude(julianDay);
        var ayanamsa = _astronomicalService.GetAyanamsa(julianDay);
        var siderealSolar = (solarLongitude - ayanamsa + 360) % 360;
        
        var masaNumber = (int)Math.Ceiling(siderealSolar / 30.0);
        if (masaNumber == 0) masaNumber = 12;
        
        var masaName = _sanskritNamesService.GetMasaName(masaNumber);
        
        // For simplicity, not calculating leap months in this implementation
        return new MasaInfo(masaNumber, masaName, false);
    }

    public async Task<SamvatsaraInfo> CalculateSamvatsaraAsync(double julianDay, int masaNumber)
    {
        await Task.CompletedTask; // For async pattern
        
        var ahargana = julianDay - KALI_YUGA_EPOCH;
        var siderealYear = 365.25636;
        
        var kali = (int)((ahargana + (4 - masaNumber) * 30) / siderealYear);
        
        // Calculate Samvatsara (60-year cycle)
        var samvatsaraNumber = ((kali + 27 + (int)((kali * 211 - 108) / 18000)) % 60) + 1;
        if (samvatsaraNumber <= 0) samvatsaraNumber += 60;
        
        var samvatsaraName = _sanskritNamesService.GetSamvatsaraName(samvatsaraNumber);
        
        return new SamvatsaraInfo(samvatsaraNumber, samvatsaraName);
    }

    public async Task<RituInfo> CalculateRituAsync(int masaNumber)
    {
        await Task.CompletedTask; // For async pattern
        
        var rituNumber = (masaNumber - 1) / 2;
        var rituName = _sanskritNamesService.GetRituName(rituNumber);
        
        return new RituInfo(rituNumber, rituName);
    }

    private TimeInDms CalculateEndTime(double sunriseJd, double degreesLeft, double degreesPerPeriod, Location location)
    {
        // Simplified end time calculation
        // In a real implementation, this would use iterative methods with astronomical calculations
        var approximateHours = (degreesLeft / degreesPerPeriod) * 24;
        var endJd = sunriseJd + approximateHours / 24;
        
        var localTime = ((endJd - Math.Floor(endJd)) * 24 + location.Timezone) % 24;
        if (localTime < 0) localTime += 24;
        
        return TimeInDms.FromDecimalHours(localTime);
    }
}
