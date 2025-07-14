using Microsoft.Extensions.Logging;
using PanchangaApi.Core.Models;

namespace PanchangaApi.Core.Services;

/// <summary>
/// Service for astronomical calculations
/// Note: This is a simplified implementation. For production use, consider integrating with Swiss Ephemeris
/// </summary>
public interface IAstronomicalService
{
    double GregorianToJulian(PanchangaDate date);
    double SolarLongitude(double julianDay);
    double LunarLongitude(double julianDay);
    double LunarLatitude(double julianDay);
    (double julianDay, TimeInDms localTime) Sunrise(double julianDay, Location location);
    (double julianDay, TimeInDms localTime) Sunset(double julianDay, Location location);
    TimeInDms? Moonrise(double julianDay, Location location);
    TimeInDms? Moonset(double julianDay, Location location);
    double LunarPhase(double julianDay);
    double GetAyanamsa(double julianDay);
}

public class AstronomicalService : IAstronomicalService
{
    private readonly ILogger<AstronomicalService> _logger;

    // Constants
    private const double J2000 = 2451545.0; // Julian day for J2000.0
    private const double TROPICAL_YEAR = 365.24219;
    private const double SYNODIC_MONTH = 29.530588853;
    private const double SIDEREAL_YEAR = 365.25636;

    public AstronomicalService(ILogger<AstronomicalService> logger)
    {
        _logger = logger;
    }

    public double GregorianToJulian(PanchangaDate date)
    {
        var dt = date.ToDateTime();
        var a = (14 - dt.Month) / 12;
        var y = dt.Year + 4800 - a;
        var m = dt.Month + 12 * a - 3;
        
        var jdn = dt.Day + (153 * m + 2) / 5 + 365 * y + y / 4 - y / 100 + y / 400 - 32045;
        return jdn;
    }

    public double SolarLongitude(double julianDay)
    {
        // Simplified calculation - for accurate results, use Swiss Ephemeris
        var t = (julianDay - J2000) / 36525.0;
        var meanLongitude = 280.46646 + 36000.76983 * t + 0.0003032 * t * t;
        var meanAnomaly = 357.52911 + 35999.05029 * t - 0.0001537 * t * t;
        
        meanAnomaly = DegToRad(meanAnomaly);
        var center = 1.914602 - 0.004817 * t - 0.000014 * t * t;
        center = center * Math.Sin(meanAnomaly);
        center += (0.019993 - 0.000101 * t) * Math.Sin(2 * meanAnomaly);
        center += 0.000289 * Math.Sin(3 * meanAnomaly);
        
        var trueLongitude = meanLongitude + center;
        return NormalizeDegrees(trueLongitude);
    }

    public double LunarLongitude(double julianDay)
    {
        // Simplified calculation - for accurate results, use Swiss Ephemeris
        var t = (julianDay - J2000) / 36525.0;
        var moonMeanLongitude = 218.3164477 + 481267.88123421 * t - 0.0015786 * t * t;
        var moonMeanElongation = 297.8501921 + 445267.1114034 * t - 0.0018819 * t * t;
        var sunMeanAnomaly = 357.5291092 + 35999.0502909 * t - 0.0001536 * t * t;
        var moonMeanAnomaly = 134.9633964 + 477198.8675055 * t + 0.0087414 * t * t;
        
        // Apply major periodic terms (simplified)
        var l = moonMeanLongitude;
        l += 6.288774 * Math.Sin(DegToRad(moonMeanAnomaly));
        l += 1.274027 * Math.Sin(DegToRad(2 * moonMeanElongation - moonMeanAnomaly));
        l += 0.658314 * Math.Sin(DegToRad(2 * moonMeanElongation));
        l -= 0.185116 * Math.Sin(DegToRad(sunMeanAnomaly));
        
        return NormalizeDegrees(l);
    }

    public double LunarLatitude(double julianDay)
    {
        // Simplified calculation - for accurate results, use Swiss Ephemeris
        var t = (julianDay - J2000) / 36525.0;
        var f = 93.2720950 + 483202.0175233 * t - 0.0036539 * t * t;
        var moonMeanAnomaly = 134.9633964 + 477198.8675055 * t + 0.0087414 * t * t;
        
        var latitude = 5.128122 * Math.Sin(DegToRad(f));
        latitude += 0.280602 * Math.Sin(DegToRad(moonMeanAnomaly + f));
        latitude += 0.277693 * Math.Sin(DegToRad(moonMeanAnomaly - f));
        
        return latitude;
    }

    public (double julianDay, TimeInDms localTime) Sunrise(double julianDay, Location location)
    {
        // Simplified sunrise calculation
        var solarDeclination = GetSolarDeclination(julianDay);
        var latRad = DegToRad(location.Latitude);
        var declRad = DegToRad(solarDeclination);
        
        var hourAngle = Math.Acos(-Math.Tan(latRad) * Math.Tan(declRad));
        var sunriseHour = 12 - RadToDeg(hourAngle) / 15.0;
        
        var localSunrise = sunriseHour + location.Timezone;
        var sunriseJd = julianDay + (sunriseHour - 12) / 24.0;
        
        return (sunriseJd, TimeInDms.FromDecimalHours(localSunrise));
    }

    public (double julianDay, TimeInDms localTime) Sunset(double julianDay, Location location)
    {
        // Simplified sunset calculation
        var solarDeclination = GetSolarDeclination(julianDay);
        var latRad = DegToRad(location.Latitude);
        var declRad = DegToRad(solarDeclination);
        
        var hourAngle = Math.Acos(-Math.Tan(latRad) * Math.Tan(declRad));
        var sunsetHour = 12 + RadToDeg(hourAngle) / 15.0;
        
        var localSunset = sunsetHour + location.Timezone;
        var sunsetJd = julianDay + (sunsetHour - 12) / 24.0;
        
        return (sunsetJd, TimeInDms.FromDecimalHours(localSunset));
    }

    public TimeInDms? Moonrise(double julianDay, Location location)
    {
        // Simplified moonrise calculation
        try
        {
            var moonriseHour = 12.0 + (Math.Sin(DegToRad(LunarLongitude(julianDay))) * 2);
            var localMoonrise = moonriseHour + location.Timezone;
            return TimeInDms.FromDecimalHours(localMoonrise);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error calculating moonrise");
            return null;
        }
    }

    public TimeInDms? Moonset(double julianDay, Location location)
    {
        // Simplified moonset calculation
        try
        {
            var moonsetHour = 24.0 - (Math.Sin(DegToRad(LunarLongitude(julianDay))) * 2);
            var localMoonset = moonsetHour + location.Timezone;
            return TimeInDms.FromDecimalHours(localMoonset);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error calculating moonset");
            return null;
        }
    }

    public double LunarPhase(double julianDay)
    {
        var solarLong = SolarLongitude(julianDay);
        var lunarLong = LunarLongitude(julianDay);
        var phase = (lunarLong - solarLong + 360) % 360;
        return phase;
    }

    public double GetAyanamsa(double julianDay)
    {
        // Lahiri ayanamsa calculation
        var t = (julianDay - J2000) / 36525.0;
        var ayanamsa = 23.85 + 0.396 * t; // Simplified Lahiri ayanamsa
        return ayanamsa;
    }

    private double GetSolarDeclination(double julianDay)
    {
        var solarLong = SolarLongitude(julianDay);
        var obliquity = 23.4393 - 0.013 * (julianDay - J2000) / 36525.0;
        return Math.Asin(Math.Sin(DegToRad(obliquity)) * Math.Sin(DegToRad(solarLong)));
    }

    private static double DegToRad(double degrees) => degrees * Math.PI / 180.0;
    private static double RadToDeg(double radians) => radians * 180.0 / Math.PI;
    private static double NormalizeDegrees(double degrees) => ((degrees % 360) + 360) % 360;
}
