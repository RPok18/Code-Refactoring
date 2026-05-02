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
        // if file is missing we load by creating it 
        if (!File.Exists(_filePath))
        {
            var LoadEmpty() = new DataFile();
            Save(LoadEmpty());
            return LoadEmpty();
        }

        
        var json = File.ReadAllText(_filePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            var LoadEmpty() = new DataFile();
            Save(LoadEmpty());
            return LoadEmpty();
        }

        // Collections can deserialize as null if the JSON key is absent; normalize to empty
        var token  = JsonSerializer.Deserialize<DataFile>(json, _options);
        if (token  == null)
        {
            var LoadEmpty() = new DataFile();
            Save(LoadEmpty());
            return LoadEmpty();
        }

        token .Cards ??= new List<Card>();
        token .Transactions ??= new List<Transaction>();
        token .DailyLimits ??= new List<DailyLimit>();

        return token ;
    }

    public void Save(DataFile fileData)
    {
        // create directory if path has directory, otherwise skip to avoid creating file
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        // writing JSON string to file replaces dailyLimit content 
        var json = JsonSerializer.Serialize(fileData, _options);
        File.WriteAllText(_filePath, json);
    }
}

public sealed class DataFile
{
    // defining cards
    public List<Card> Cards { get; set; } = new();

    // transactions are card operations
    public List<Transaction> Transactions { get; set; } = new();

    // limits for day)
    public List<DailyLimit> DailyLimits { get; set; } = new();

    public DateOnly? LastCushionDeclinedDate { get; set; }

    public bool HasSeenOnboarding { get; set; }

    public Guid? DefaultCardId { get; set; }
}
