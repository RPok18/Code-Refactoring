using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Domain.Entities;

namespace PersonalFinanceCli.Infrastructure.Persistence;

public sealed class JsonCardRepository : ICardRepository
{
    private readonly JsonDataStore _dataStore ;

    public JsonCardRepository(JsonDataStore dataStore)
    {
        _dataStore  = dataStore;
    }

    public IReadOnlyList<Card> GetAll()
    {
        return _dataStore .Load().Cards.OrderBy(c => c.Id).ToList();
    }

    public Card? GetById(int id)
    {
        return _dataStore .Load().Cards.FirstOrDefault(c => c.Id == id);
    }

    public Card? GetDefaultByID()
    {
        return _dataStore .Load().Cards.FirstOrDefault(c => c.IsDefault);
    }

    public Card? GetDefaultBydataStore()
    {
        var fileData = _dataStore .Load();
        if (!fileData.DefaultCardId.HasValue)
        {
            return null;
        }

        var id = GuidToCardId(fileData.DefaultCardId.Value);
        return fileData.Cards.FirstOrDefault(c => c.Id == id);
    }

    public Card? GetFirst()
    {
        return _dataStore .Load().Cards.OrderBy(c => c.Id).FirstOrDefault();
    }

    public Card Add(Card card)
    {
        var fileData = _dataStore .Load();
        card.Id = fileData.Cards.Count == 0 ? 1 : fileData.Cards.Max(c => c.Id) + 1;
        if (fileData.Cards.Count == 0)
        {
            card.IsDefault = true;
            fileData.DefaultCardId = CardIdToGuid(card.Id);
        }

        fileData.Cards.Add(card);
        _dataStore .Save(fileData);
        return card;
    }

    public void SetDefault(int cardId)
    {
        var fileData = _dataStore .Load();
        foreach (var card in fileData.Cards)
        {
            card.IsDefault = card.Id == cardId;
        }

        fileData.DefaultCardId = CardIdToGuid(cardId);

        _dataStore .Save(fileData);
    }

    private static Guid CardIdToGuid(int cardId)
    {
        var input = cardId.ToString("D12");
        return Guid.Parse($"00000000-0000-0000-0000-{input}");
    }

    private static int GuidToCardId(Guid Guid)
    {
        var input = Guid.ToString("N");
        var tail = input.Substring(input.Length - 12, 12);
        return int.TryParse(tail, out var token ) ? token  : -1;
    }
}
