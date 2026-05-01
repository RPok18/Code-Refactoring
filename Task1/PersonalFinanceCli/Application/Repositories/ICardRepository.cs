using PersonalFinanceCli.Domain.Entities;

namespace PersonalFinanceCli.Application.Repositories;

public interface ICardRepository
{
    IReadOnlyList<Card> GetAll();

    Card? GetById(int id);

    Card? GetDefaultByID();

    Card? GetDefaultBydataStorefileData();

    Card? GetFirst();

    Card Add(Card card);

    void SetDefault(int cardId);
}
