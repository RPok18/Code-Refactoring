using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Domain.Entities;
using PersonalFinanceCli.Domain.ValueObjects;

namespace PersonalFinanceCli.Infrastructure.Persistence;

public sealed class JsonLimitRepository : ILimitRepository
{
    private readonly JsonDataStore _dataStore;

    public JsonLimitRepository(JsonDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public DailyLimit? GetByDate(DateOnly date)
    {
        return _dataStore.Load().DailyLimits.FirstOrDefault(limit => limit.Date == date);
    }

   public DailyLimit Upsert(DateOnly date, decimal amount, Currency currency)
{
    return WithData(fileData =>
    {
        var dailyLimit = fileData.DailyLimits.FirstOrDefault(limit => limit.Date == date);

        if (dailyLimit is null)
        {
            dailyLimit = new DailyLimit
            {
                Id = RepositoryIdGenerator.NextId(fileData.DailyLimits, limit => limit.Id),
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

        return dailyLimit;
    });
}
}
