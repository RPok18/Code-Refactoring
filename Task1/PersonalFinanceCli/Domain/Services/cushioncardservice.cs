using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Domain.Entities;

namespace PersonalFinanceCli.Application.Services;

public sealed class CushionCardFinder : ICushionCardFinder
{
    private readonly ICardRepository _cardRepository;

    public CushionCardFinder(ICardRepository cardRepository)
    {
        _cardRepository = cardRepository;
    }

    public Card? FindCushion()
    {
        var cards = _cardRepository.GetAll();

        
        var byFlag = cards.FirstOrDefault(c => c.IsCushion);
        if (byFlag != null)
        {
            return byFlag;
        }

       
        var byExactName = cards.FirstOrDefault(c => c.Name == "Financial cushion");
        if (byExactName != null)
        {
            return byExactName;
        }

       
        return cards.FirstOrDefault(c => c.Name.Contains("cushion", StringComparison.OrdinalIgnoreCase));
    }

   
    public bool CushionExists() => FindCushion() != null;
}