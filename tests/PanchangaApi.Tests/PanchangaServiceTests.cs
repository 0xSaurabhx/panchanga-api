using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using PanchangaApi.Core.Models;
using PanchangaApi.Core.Services;

namespace PanchangaApi.Tests.Services;

public class PanchangaServiceTests
{
    private readonly Mock<IAstronomicalService> _mockAstronomicalService;
    private readonly Mock<ISanskritNamesService> _mockSanskritNamesService;
    private readonly Mock<ILogger<PanchangaService>> _mockLogger;
    private readonly PanchangaService _panchangaService;

    public PanchangaServiceTests()
    {
        _mockAstronomicalService = new Mock<IAstronomicalService>();
        _mockSanskritNamesService = new Mock<ISanskritNamesService>();
        _mockLogger = new Mock<ILogger<PanchangaService>>();
        
        _panchangaService = new PanchangaService(
            _mockAstronomicalService.Object,
            _mockSanskritNamesService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CalculatePanchangaAsync_ValidInput_ReturnsValidPanchangaData()
    {
        // Arrange
        var date = new PanchangaDate(2024, 1, 15);
        var location = new Location(12.9716, 77.5946, 5.5, "Bangalore");
        var julianDay = 2460319.5;

        _mockAstronomicalService.Setup(x => x.GregorianToJulian(date)).Returns(julianDay);
        _mockAstronomicalService.Setup(x => x.Sunrise(julianDay, location))
            .Returns((julianDay + 0.25, new TimeInDms(6, 30, 0)));
        _mockAstronomicalService.Setup(x => x.Sunset(julianDay, location))
            .Returns((julianDay + 0.75, new TimeInDms(18, 30, 0)));
        _mockAstronomicalService.Setup(x => x.LunarPhase(It.IsAny<double>())).Returns(150.0);
        _mockAstronomicalService.Setup(x => x.LunarLongitude(It.IsAny<double>())).Returns(120.0);
        _mockAstronomicalService.Setup(x => x.SolarLongitude(It.IsAny<double>())).Returns(295.0);
        _mockAstronomicalService.Setup(x => x.GetAyanamsa(It.IsAny<double>())).Returns(24.0);

        _mockSanskritNamesService.Setup(x => x.GetTithiName(It.IsAny<int>())).Returns("Test Tithi");
        _mockSanskritNamesService.Setup(x => x.GetNakshatraName(It.IsAny<int>())).Returns("Test Nakshatra");
        _mockSanskritNamesService.Setup(x => x.GetYogaName(It.IsAny<int>())).Returns("Test Yoga");
        _mockSanskritNamesService.Setup(x => x.GetKaranaName(It.IsAny<int>())).Returns("Test Karana");
        _mockSanskritNamesService.Setup(x => x.GetVaraName(It.IsAny<int>())).Returns("Test Vara");
        _mockSanskritNamesService.Setup(x => x.GetMasaName(It.IsAny<int>())).Returns("Test Masa");
        _mockSanskritNamesService.Setup(x => x.GetSamvatsaraName(It.IsAny<int>())).Returns("Test Samvatsara");
        _mockSanskritNamesService.Setup(x => x.GetRituName(It.IsAny<int>())).Returns("Test Ritu");

        // Act
        var result = await _panchangaService.CalculatePanchangaAsync(date, location);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(date, result.Date);
        Assert.Equal(location, result.Location);
        Assert.Equal("Test Tithi", result.Tithi.Name);
        Assert.Equal("Test Nakshatra", result.Nakshatra.Name);
        Assert.Equal("Test Yoga", result.Yoga.Name);
        Assert.Equal("Test Karana", result.Karana.Name);
        Assert.Equal("Test Vara", result.Vara.Name);
        Assert.Equal("Test Masa", result.Masa.Name);
        Assert.Equal("Test Samvatsara", result.Samvatsara.Name);
        Assert.Equal("Test Ritu", result.Ritu.Name);
    }

    [Fact]
    public async Task CalculatePanchangaAsync_InvalidDate_ThrowsArgumentException()
    {
        // Arrange
        var invalidDate = new PanchangaDate(2024, 13, 32); // Invalid date
        var location = new Location(12.9716, 77.5946, 5.5, "Bangalore");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _panchangaService.CalculatePanchangaAsync(invalidDate, location));
    }

    [Fact]
    public async Task CalculatePanchangaAsync_InvalidLocation_ThrowsArgumentException()
    {
        // Arrange
        var date = new PanchangaDate(2024, 1, 15);
        var invalidLocation = new Location(91.0, 181.0, 25.0); // Invalid coordinates

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _panchangaService.CalculatePanchangaAsync(date, invalidLocation));
    }

    [Theory]
    [InlineData(150.0, 13)] // 150 degrees / 12 = 12.5, ceiling = 13
    [InlineData(0.0, 30)]   // 0 degrees should map to tithi 30
    [InlineData(360.0, 30)] // 360 degrees should map to tithi 30
    [InlineData(180.0, 15)] // 180 degrees / 12 = 15
    public async Task CalculateTithiAsync_VariousPhases_ReturnsCorrectTithi(double moonPhase, int expectedTithi)
    {
        // Arrange
        var location = new Location(12.9716, 77.5946, 5.5, "Bangalore");
        var julianDay = 2460319.5;

        _mockAstronomicalService.Setup(x => x.Sunrise(julianDay, location))
            .Returns((julianDay + 0.25, new TimeInDms(6, 30, 0)));
        _mockAstronomicalService.Setup(x => x.LunarPhase(It.IsAny<double>())).Returns(moonPhase);
        _mockSanskritNamesService.Setup(x => x.GetTithiName(expectedTithi)).Returns($"Tithi {expectedTithi}");

        // Act
        var result = await _panchangaService.CalculateTithiAsync(julianDay, location);

        // Assert
        Assert.Equal(expectedTithi, result.Number);
        Assert.Equal($"Tithi {expectedTithi}", result.Name);
    }
}

public class AstronomicalServiceTests
{
    private readonly AstronomicalService _astronomicalService;
    private readonly Mock<ILogger<AstronomicalService>> _mockLogger;

    public AstronomicalServiceTests()
    {
        _mockLogger = new Mock<ILogger<AstronomicalService>>();
        _astronomicalService = new AstronomicalService(_mockLogger.Object);
    }

    [Theory]
    [InlineData(2024, 1, 1, 2460311)]
    [InlineData(2000, 1, 1, 2451545)]
    [InlineData(1900, 1, 1, 2415021)]
    public void GregorianToJulian_ValidDates_ReturnsCorrectJulianDay(int year, int month, int day, double expectedJd)
    {
        // Arrange
        var date = new PanchangaDate(year, month, day);

        // Act
        var result = _astronomicalService.GregorianToJulian(date);

        // Assert
        Assert.Equal(expectedJd, result, 1); // Allow 1 day tolerance for calculation differences
    }

    [Fact]
    public void SolarLongitude_ValidJulianDay_ReturnsValidLongitude()
    {
        // Arrange
        var julianDay = 2460319.5; // January 15, 2024

        // Act
        var result = _astronomicalService.SolarLongitude(julianDay);

        // Assert
        Assert.True(result >= 0 && result < 360, $"Solar longitude should be between 0 and 360, but got {result}");
    }

    [Fact]
    public void LunarLongitude_ValidJulianDay_ReturnsValidLongitude()
    {
        // Arrange
        var julianDay = 2460319.5; // January 15, 2024

        // Act
        var result = _astronomicalService.LunarLongitude(julianDay);

        // Assert
        Assert.True(result >= 0 && result < 360, $"Lunar longitude should be between 0 and 360, but got {result}");
    }

    [Fact]
    public void LunarPhase_ValidJulianDay_ReturnsValidPhase()
    {
        // Arrange
        var julianDay = 2460319.5; // January 15, 2024

        // Act
        var result = _astronomicalService.LunarPhase(julianDay);

        // Assert
        Assert.True(result >= 0 && result < 360, $"Lunar phase should be between 0 and 360, but got {result}");
    }
}

public class SanskritNamesServiceTests
{
    private readonly SanskritNamesService _sanskritNamesService;
    private readonly Mock<ILogger<SanskritNamesService>> _mockLogger;

    public SanskritNamesServiceTests()
    {
        _mockLogger = new Mock<ILogger<SanskritNamesService>>();
        _sanskritNamesService = new SanskritNamesService(_mockLogger.Object);
    }

    [Theory]
    [InlineData(0, "Bhānuvāra")]
    [InlineData(1, "Somavāra")]
    [InlineData(6, "Śanivāra")]
    public void GetVaraName_ValidNumbers_ReturnsCorrectNames(int number, string expectedName)
    {
        // Act
        var result = _sanskritNamesService.GetVaraName(number);

        // Assert
        Assert.Equal(expectedName, result);
    }

    [Theory]
    [InlineData(1, "Caitra")]
    [InlineData(12, "Phālguṇa")]
    public void GetMasaName_ValidNumbers_ReturnsCorrectNames(int number, string expectedName)
    {
        // Act
        var result = _sanskritNamesService.GetMasaName(number);

        // Assert
        Assert.Equal(expectedName, result);
    }

    [Theory]
    [InlineData(0, "Vasanta")]
    [InlineData(5, "Śiśira")]
    public void GetRituName_ValidNumbers_ReturnsCorrectNames(int number, string expectedName)
    {
        // Act
        var result = _sanskritNamesService.GetRituName(number);

        // Assert
        Assert.Equal(expectedName, result);
    }

    [Fact]
    public void GetTithiName_InvalidNumber_ReturnsDefaultName()
    {
        // Act
        var result = _sanskritNamesService.GetTithiName(999);

        // Assert
        Assert.Equal("Tithi-999", result);
    }
}
