using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Domain.Entities;
namespace PersonalFinanceCli.Infrastructure.Persistence;
public sealed class JsonTransactionRepository : ITransactionRepository
{
    private readonly JsonDataStore _dataStore;
    public JsonTransactionRepository(JsonDataStore dataStore)
    {
        _dataStore = dataStore;
    }
    public IReadOnlyList<Transaction> GetAll()
    {
        return _dataStore.Load().Transactions.OrderBy(t => t.Id).ToList();
    }
    public Transaction Add(Transaction transaction)
    {
        var fileData = _dataStore.Load();
        transaction.Id = fileData.Transactions.Count == 0 ? 1 : fileData.Transactions.Max(t => t.Id) + 1;
        fileData.Transactions.Add(transaction);
        _dataStore.Save(fileData);
        return transaction;
    }
}