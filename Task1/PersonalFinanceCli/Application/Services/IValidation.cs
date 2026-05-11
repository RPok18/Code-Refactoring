using PersonalFinanceCli.Domain.Entities;
using PersonalFinanceCli.Domain.ValueObjects;

namespace PersonalFinanceCli.Application.Services;

public interface IValidator
{
    void Validate(object value);
}

public interface ICardResolver
{
    int ResolveCardId(int? cardId, TransactionType type);
}

public interface ICushionCardFinder
{
    Card? FindCushion();
}