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
    return WithData(fileData =>
    {
        transaction.Id = RepositoryIdGenerator.NextId(fileData.Transactions, t => t.Id);
        fileData.Transactions.Add(transaction);
        return transaction;
    });
}
}