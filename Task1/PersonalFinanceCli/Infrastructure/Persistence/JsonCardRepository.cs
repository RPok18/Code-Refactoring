using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Domain.Entities;

namespace PersonalFinanceCli.Infrastructure.Persistence;

public sealed class JsonCardRepository : JsonRepositoryloadsave<FileDataType>, ICardRepository
{
    private readonly JsonDataStore _dataStore;

    public JsonCardRepository(JsonDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public IReadOnlyList<Card> GetAll()
    {
        return _dataStore.Load().Cards.OrderBy(c => c.Id).ToList();
    }

    public Card? GetById(int id)
    {
        return _dataStore.Load().Cards.FirstOrDefault(c => c.Id == id);
    }

    public Card? GetDefaultCard()
    {
        return _dataStore.Load().Cards.FirstOrDefault(c => c.IsDefault);
    }

    public Card? GetDefaultCardByStoredId()
    {
        var fileData = _dataStore.Load();
        if (!fileData.DefaultCardId.HasValue)
        {
            return null;
        }

        var id = GuidToCardId(fileData.DefaultCardId.Value);
        return fileData.Cards.FirstOrDefault(c => c.Id == id);
    }

    public Card? GetFirst()
    {
        return _dataStore.Load().Cards.OrderBy(c => c.Id).FirstOrDefault();
    }

   public Card Add(Card card)
{
    return WithData(fileData =>
    {
        card.Id = RepositoryIdGenerator.NextId(fileData.Cards, c => c.Id);

        if (fileData.Cards.Count == 0)
        {
            card.IsDefault = true;
            fileData.DefaultCardId = CardIdToGuid(card.Id);
        }

        fileData.Cards.Add(card);
        return card;
    });
}

   public void SetDefault(int cardId)
{
    WithData(fileData =>
    {
        foreach (var card in fileData.Cards)
        {
            card.IsDefault = card.Id == cardId;
        }

        fileData.DefaultCardId = CardIdToGuid(cardId);
    });
}

    private static Guid CardIdToGuid(int cardId)
    {
        var input = cardId.ToString("D12");
        return Guid.Parse($"00000000-0000-0000-0000-{input}");
    }

    private static int GuidToCardId(Guid guid)
    {
        var input = guid.ToString("N");
        var tail = input[^12..];
        return int.TryParse(tail, out var id) ? id : -1;
    }
}