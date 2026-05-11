using PersonalFinanceCli.Domain.Entities;
namespace PersonalFinanceCli.Application.Repositories;
public interface ILimitRepository
{
    DailyLimit? GetByDate(DateOnly date);
    void UpsertDailyLimit(DateOnly date, decimal amount, Domain.ValueObjects.Currency currency);
}