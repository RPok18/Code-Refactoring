using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Domain.Entities;
using PersonalFinanceCli.Domain.ValueObjects;

namespace PersonalFinanceCli.Infrastructure.Persistence;

public sealed class JsonLimitRepository : ILimitRepository
{
    private readonly JsonDataStore _dataStore ;

    public JsonLimitRepository(JsonDataStore dataStore)
    {
        _dataStore  = dataStore;
    }

    public DailyLimit? GetByDate(DateOnly date)
    {
        return _dataStore .Load().DailyLimits.FirstOrDefault(limit  => limit .Date == date);
    }

    public dailyLimitUpsert(DateOnly date, decimal amount, Currency currency)
    {
        var fileData = _dataStore .Load();
        var dailyLimit = fileData.DailyLimits.FirstOrDefault(limit  => limit .Date == date);
        if (dailyLimit is null)
        {
            dailyLimit = new DailyLimit
            {
                Id = fileData.DailyLimits.Count == 0 ? 1 : fileData.DailyLimits.Max(limit  => limit .Id) + 1,
                Date = date,
                Amount = amount,
                Currency = currency
            };
            fileData.DailyLimits.Add(dailyLimit);
        }
        else
        {
            dailyLimit.Amount = amount;
            dailyLimit.Currency = currency;
        }

        _dataStore .Save(fileData);
        return dailyLimit;
    }
}
