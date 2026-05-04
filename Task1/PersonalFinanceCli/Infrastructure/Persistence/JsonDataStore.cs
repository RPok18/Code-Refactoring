using System.Text.Json;
using PersonalFinanceCli.Domain.Entities;

namespace PersonalFinanceCli.Infrastructure.Persistence;

public sealed class JsonDataStore
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options;

    public JsonDataStore(string filePath)
    {
        _filePath = filePath;
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public DataFile Load()
    {
        DataFile CreateAndSaveEmpty()
        {
            var empty = new DataFile();
            Save(empty);
            return empty;
        }

        if (!File.Exists(_filePath))
        {
            return CreateAndSaveEmpty();
        }

        var json = File.ReadAllText(_filePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return CreateAndSaveEmpty();
        }

        // Collections can deserialize as null if the JSON key is absent; normalize to empty
        var data = JsonSerializer.Deserialize<DataFile>(json, _options);
        if (data == null)
        {
            return CreateAndSaveEmpty();
        }

        data.Cards ??= new List<Card>();
        data.Transactions ??= new List<Transaction>();
        data.DailyLimits ??= new List<DailyLimit>();
        return data;
    }

    public void Save(DataFile fileData)
    {
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var json = JsonSerializer.Serialize(fileData, _options);
        File.WriteAllText(_filePath, json);
    }
}

public sealed class DataFile
{
    public List<Card> Cards { get; set; } = new();
    public List<Transaction> Transactions { get; set; } = new();
    public List<DailyLimit> DailyLimits { get; set; } = new();
    public DateOnly? LastCushionDeclinedDate { get; set; }
    public bool HasSeenOnboarding { get; set; }
    public Guid? DefaultCardId { get; set; }
}