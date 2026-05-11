using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Infrastructure.Time;
namespace PersonalFinanceCli.Presentation.Rendering;
public sealed class ConsoleDisplay                          
{
    private readonly IConsole _console;
    private readonly ICardRepository _cardRepository;
    private readonly ILimitRepository _limitRepository;
    private readonly IClock _clock;
    public ConsoleDisplay(
        IConsole console,
        ICardRepository cardRepository,
        ILimitRepository limitRepository,
        IClock clock)
    {
        _console = console;
        _cardRepository = cardRepository;
        _limitRepository = limitRepository;
        _clock = clock;
    }
    public void PrintHelp()
    {
        _console.WriteLine("Commands:");
        _console.WriteLine("  help");
        _console.WriteLine("  exit");
        _console.WriteLine("  card add \"Name\" <RUB|EUR> [initialBalance]");
        _console.WriteLine("  card list");
        _console.WriteLine("  card set-default <cardId>");
        _console.WriteLine("  expense add <amount> <category> [--card <id>] [--date YYYY-MM-DD] [--note \"text\"]");
        _console.WriteLine("  income add <amount> <category> [--card <id>] [--date YYYY-MM-DD] [--note \"text\"]");
        _console.WriteLine("  limit set <amount>");
        _console.WriteLine("  limit show");
        _console.WriteLine("  report day [--date YYYY-MM-DD]");
    }
    public void PrintCards()
    {
        var cards = _cardRepository.GetAll();
        if (cards.Count == 0)
        {
            _console.WriteLine("Cards: (none)");
            return;
        }
        _console.WriteLine("Cards:");
        foreach (var card in cards)
        {
            var marker = card.IsDefault ? " (default)" : string.Empty;
            _console.WriteLine($"  {card.Id}: {card.Name}{marker} [{card.Currency}] {card.InitialBalance:F2}");
        }
    }
    public void ShowLimit()
    {
        var today = _clock.Today;
        var limit = _limitRepository.GetByDate(today);     
        if (limit is null)
        {
            _console.WriteLine("Limit: (not set)");
            return;
        }
        var cards = _cardRepository.GetAll();
        var currency = cards.FirstOrDefault(c => c.IsDefault)?.Currency
            ?? cards.FirstOrDefault()?.Currency
            ?? limit.Currency;
        _console.WriteLine($"Limit: {limit.Amount:F2} {currency} ({today:yyyy-MM-dd})");
    }
}