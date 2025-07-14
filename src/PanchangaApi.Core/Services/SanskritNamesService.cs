using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace PanchangaApi.Core.Services;

/// <summary>
/// Service for managing Sanskrit names for various Panchanga elements
/// </summary>
public interface ISanskritNamesService
{
    string GetMasaName(int number);
    string GetTithiName(int number);
    string GetNakshatraName(int number);
    string GetYogaName(int number);
    string GetKaranaName(int number);
    string GetVaraName(int number);
    string GetSamvatsaraName(int number);
    string GetRituName(int number);
}

public class SanskritNamesService : ISanskritNamesService
{
    private readonly Dictionary<string, Dictionary<string, string>> _names;
    private readonly ILogger<SanskritNamesService> _logger;

    public SanskritNamesService(ILogger<SanskritNamesService> logger)
    {
        _logger = logger;
        _names = LoadSanskritNames();
    }

    public string GetMasaName(int number)
    {
        return GetName("masas", number.ToString()) ?? $"Masa-{number}";
    }

    public string GetTithiName(int number)
    {
        return GetName("tithis", number.ToString()) ?? $"Tithi-{number}";
    }

    public string GetNakshatraName(int number)
    {
        return GetName("nakshatras", number.ToString()) ?? $"Nakshatra-{number}";
    }

    public string GetYogaName(int number)
    {
        return GetName("yogas", number.ToString()) ?? $"Yoga-{number}";
    }

    public string GetKaranaName(int number)
    {
        return GetName("karanas", number.ToString()) ?? $"Karana-{number}";
    }

    public string GetVaraName(int number)
    {
        return GetName("varas", number.ToString()) ?? $"Vara-{number}";
    }

    public string GetSamvatsaraName(int number)
    {
        return GetName("samvats", number.ToString()) ?? $"Samvatsara-{number}";
    }

    public string GetRituName(int number)
    {
        return GetName("ritus", number.ToString()) ?? $"Ritu-{number}";
    }

    private string? GetName(string category, string key)
    {
        if (_names.TryGetValue(category, out var categoryDict) &&
            categoryDict.TryGetValue(key, out var name))
        {
            return name;
        }
        return null;
    }

    private Dictionary<string, Dictionary<string, string>> LoadSanskritNames()
    {
        try
        {
            // Try to find the Sanskrit names file in various locations
            var possiblePaths = new[]
            {
                "sanskrit-names.json",
                "../../../sanskrit-names.json",
                "../../../../sanskrit-names.json",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sanskrit-names.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "sanskrit-names.json")
            };

            string? jsonContent = null;
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    jsonContent = File.ReadAllText(path);
                    _logger.LogInformation("Loaded Sanskrit names from: {Path}", path);
                    break;
                }
            }

            if (jsonContent == null)
            {
                _logger.LogWarning("Sanskrit names file not found. Using default names.");
                return GetDefaultNames();
            }

            var names = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonContent);
            return names ?? GetDefaultNames();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Sanskrit names. Using defaults.");
            return GetDefaultNames();
        }
    }

    private static Dictionary<string, Dictionary<string, string>> GetDefaultNames()
    {
        return new Dictionary<string, Dictionary<string, string>>
        {
            ["masas"] = new()
            {
                ["1"] = "Caitra", ["2"] = "Vaiśākha", ["3"] = "Jyeṣṭha", ["4"] = "Āṣāḍha",
                ["5"] = "Śrāvaṇa", ["6"] = "Bhādrapada", ["7"] = "Āśvina", ["8"] = "Kārtika",
                ["9"] = "Mārgaśīrṣa", ["10"] = "Puṣya", ["11"] = "Māgha", ["12"] = "Phālguṇa"
            },
            ["varas"] = new()
            {
                ["0"] = "Bhānuvāra", ["1"] = "Somavāra", ["2"] = "Maṅgalavāra", ["3"] = "Budhavāra",
                ["4"] = "Guruvāra", ["5"] = "Śukravāra", ["6"] = "Śanivāra"
            },
            ["tithis"] = new(),
            ["nakshatras"] = new(),
            ["yogas"] = new(),
            ["karanas"] = new(),
            ["samvats"] = new(),
            ["ritus"] = new()
            {
                ["0"] = "Vasanta", ["1"] = "Grīṣma", ["2"] = "Varṣā",
                ["3"] = "Śarad", ["4"] = "Hemanta", ["5"] = "Śiśira"
            }
        };
    }
}
