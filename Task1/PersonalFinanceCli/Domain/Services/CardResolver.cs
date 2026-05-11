using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Domain.Entities;
using PersonalFinanceCli.Domain.ValueObjects;

namespace PersonalFinanceCli.Application.Services;

public class CardResolver : ICardResolver
{
    private readonly ICardRepository _cardRepository;

    public CardResolver(ICardRepository cardRepository)
    {
        _cardRepository = cardRepository;
    }

    public int ResolveCardId(int? cardId, TransactionType type)
    {
        if (cardId.HasValue)
        {
            var byId = _cardRepository.GetById(cardId.Value);
            if (byId == null)
            {
                throw new InvalidOperationException("Card not found.");
            }
            return byId.Id;
        }

        var defaultCard = _cardRepository.GetDefaultBydataStorefileData();
        if (defaultCard != null)
        {
            return defaultCard.Id;
        }

        var first = _cardRepository.GetFirst();
        if (first != null)
        {
            return first.Id;
        }

        throw new InvalidOperationException("No cards available.");
    }
}