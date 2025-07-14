namespace PanchangaApi.Core.Models;

/// <summary>
/// Represents a geographical location with coordinates and timezone
/// </summary>
public record Location(
    double Latitude,
    double Longitude,
    double Timezone,
    string? Name = null)
{
    /// <summary>
    /// Validates if the location coordinates are within valid ranges
    /// </summary>
    public bool IsValid => 
        Latitude >= -90 && Latitude <= 90 && 
        Longitude >= -180 && Longitude <= 180 &&
        Timezone >= -12 && Timezone <= 14;
}

/// <summary>
/// Represents a date in the Gregorian calendar
/// </summary>
public record PanchangaDate(
    int Year,
    int Month,
    int Day)
{
    /// <summary>
    /// Validates if the date is a valid Gregorian date
    /// </summary>
    public bool IsValid
    {
        get
        {
            try
            {
                _ = new DateTime(Year, Month, Day);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Converts to DateTime
    /// </summary>
    public DateTime ToDateTime() => new(Year, Month, Day);
}

/// <summary>
/// Represents time in degrees, minutes, and seconds format
/// </summary>
public record TimeInDms(
    int Degrees,
    int Minutes,
    int Seconds)
{
    /// <summary>
    /// Converts DMS to decimal hours
    /// </summary>
    public double ToDecimalHours() => Degrees + Minutes / 60.0 + Seconds / 3600.0;

    /// <summary>
    /// Creates DMS from decimal hours
    /// </summary>
    public static TimeInDms FromDecimalHours(double hours)
    {
        var degrees = (int)hours;
        var remainingMinutes = (hours - degrees) * 60;
        var minutes = (int)remainingMinutes;
        var seconds = (int)Math.Round((remainingMinutes - minutes) * 60);
        
        return new TimeInDms(degrees, minutes, seconds);
    }

    public override string ToString() => $"{Degrees:D2}:{Minutes:D2}:{Seconds:D2}";
}

/// <summary>
/// Represents a Tithi with its number and end time
/// </summary>
public record TithiInfo(
    int Number,
    string Name,
    TimeInDms? EndTime = null,
    bool IsSkipped = false);

/// <summary>
/// Represents a Nakshatra with its number and end time
/// </summary>
public record NakshatraInfo(
    int Number,
    string Name,
    TimeInDms? EndTime = null,
    bool IsSkipped = false);

/// <summary>
/// Represents a Yoga with its number and end time
/// </summary>
public record YogaInfo(
    int Number,
    string Name,
    TimeInDms? EndTime = null,
    bool IsSkipped = false);

/// <summary>
/// Represents a Karana with its number
/// </summary>
public record KaranaInfo(
    int Number,
    string Name);

/// <summary>
/// Represents a weekday (Vara)
/// </summary>
public record VaraInfo(
    int Number,
    string Name);

/// <summary>
/// Represents a lunar month (Masa) with leap month information
/// </summary>
public record MasaInfo(
    int Number,
    string Name,
    bool IsLeapMonth = false);

/// <summary>
/// Represents a Samvatsara (60-year cycle)
/// </summary>
public record SamvatsaraInfo(
    int Number,
    string Name);

/// <summary>
/// Represents a Ritu (season)
/// </summary>
public record RituInfo(
    int Number,
    string Name);

/// <summary>
/// Comprehensive Panchanga information for a given date and location
/// </summary>
public record PanchangaData(
    PanchangaDate Date,
    Location Location,
    TithiInfo Tithi,
    NakshatraInfo Nakshatra,
    YogaInfo Yoga,
    KaranaInfo Karana,
    VaraInfo Vara,
    MasaInfo Masa,
    SamvatsaraInfo Samvatsara,
    RituInfo Ritu,
    TimeInDms Sunrise,
    TimeInDms Sunset,
    TimeInDms? Moonrise = null,
    TimeInDms? Moonset = null,
    double DayDurationHours = 0)
{
    /// <summary>
    /// Additional Tithi if there's a skipped one
    /// </summary>
    public TithiInfo? AdditionalTithi { get; init; }

    /// <summary>
    /// Additional Nakshatra if there's a skipped one
    /// </summary>
    public NakshatraInfo? AdditionalNakshatra { get; init; }

    /// <summary>
    /// Additional Yoga if there's a skipped one
    /// </summary>
    public YogaInfo? AdditionalYoga { get; init; }
}
