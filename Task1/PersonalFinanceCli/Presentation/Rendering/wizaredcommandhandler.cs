using PersonalFinanceCli.Application.CommandHandlers;
using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Application.Services;
using PersonalFinanceCli.Domain.Services;
using PersonalFinanceCli.Infrastructure.Time;
using PersonalFinanceCli.Presentation.Parsing;

namespace PersonalFinanceCli.Presentation.Rendering;

public sealed class WizardCommandHandler
{
    private readonly InputPrompter _prompter;
    private readonly AddCardHandler _addCardHandler;
    private readonly AddTransactionHandler _addTransactionHandler;
    private readonly AddIncomeHandler _addIncomeHandler;
    private readonly AddExpenseHandler _addExpenseHandler;
    private readonly SetDailyLimitHandler _setDailyLimitHandler;
    private readonly DailyReportService _dailyReportService;
    private readonly ReportPrinter _reportPrinter;
    private readonly IClock _clock;
    private readonly IConsole _console;
    private readonly CushionTransferWizard _cushionTransferWizard;  // replaces 4 deps
    private readonly WizardOptionCollector _wizardOptionCollector;  // now injected

    public WizardCommandHandler(
        InputPrompter prompter,
        AddCardHandler addCardHandler,
        AddTransactionHandler addTransactionHandler,
        AddIncomeHandler addIncomeHandler,
        AddExpenseHandler addExpenseHandler,
        SetDailyLimitHandler setDailyLimitHandler,
        DailyReportService dailyReportService,
        ReportPrinter reportPrinter,
        IClock clock,
        IConsole console,
        CushionTransferWizard cushionTransferWizard,
        WizardOptionCollector wizardOptionCollector)
    {
        _prompter = prompter;
        _addCardHandler = addCardHandler;
        _addTransactionHandler = addTransactionHandler;
        _addIncomeHandler = addIncomeHandler;
        _addExpenseHandler = addExpenseHandler;
        _setDailyLimitHandler = setDailyLimitHandler;
        _dailyReportService = dailyReportService;
        _reportPrinter = reportPrinter;
        _clock = clock;
        _console = console;
        _cushionTransferWizard = cushionTransferWizard;
        _wizardOptionCollector = wizardOptionCollector;
    }

    public bool TryHandleWizard(string line)
    {
        var tokens = Tokenizer.Tokenize(line);
        if (tokens.Count < 2) return false;

        var root = tokens[0].ToLowerInvariant();
        var action = tokens[1].ToLowerInvariant();

        if (root == "card"    && action == "add") { HandleCardAddWizard(tokens);    return true; }
        if (root == "expense" && action == "add") { HandleExpenseAddWizard(tokens); return true; }
        if (root == "income"  && action == "add") { HandleIncomeAddWizard(tokens);  return true; }
        if (root == "limit"   && action == "set") { HandleLimitSetWizard(tokens);   return true; }

        return false;
    }

    private void RunWizard(Action wizard)
    {
        try { wizard(); }
        catch (WizardCancelledException) { _console.WriteLine("Cancelled."); }
        catch (Exception ex)
        {
            _console.WriteLine($"Error: {ex.Message}");
            _console.WriteLine("type help");
        }
    }

    private record TransactionInputs(
        decimal Amount, string Category, int? CardId, DateOnly? Date, string? Note);

    private TransactionInputs? CollectTransactionInputs(IReadOnlyList<string> tokens)
    {
        var amount = _prompter.AskRequiredDecimal(tokens.Count >= 3 ? tokens[2] : null, "Amount?");

        var categoryToken = tokens.Count >= 4 ? tokens[3] : null;
        var optionsIndex = 4;
        if (categoryToken != null && categoryToken.StartsWith("--", StringComparison.Ordinal))
        {
            categoryToken = null;
            optionsIndex = 3;
        }

        var options = _wizardOptionCollector.Collect(tokens, optionsIndex);
        if (options.Error != null)
        {
            _console.WriteLine($"Error: {options.Error}");
            _console.WriteLine("type help");
            return null;
        }

        var category = _prompter.AskRequiredText(categoryToken, "Category?");
        var cardId   = _prompter.ResolveCardWizard(options.CardguidHex, "Card? (enter to use default, id or name)");
        var date     = options.Date ?? _prompter.AskOptionalDate(null, "Date? (YYYY-MM-DD, enter = today)");

        return new TransactionInputs(amount, category, cardId, date, options.Note);
    }

    private void HandleCardAddWizard(IReadOnlyList<string> tokens) => RunWizard(() =>
    {
        var name           = _prompter.AskRequiredText(tokens.Count >= 3 ? tokens[2] : null, "Card name?");
        var currency       = _prompter.AskCurrency(tokens.Count >= 4 ? tokens[3] : null);
        var initialBalance = _prompter.AskOptionalDecimal(tokens.Count >= 5 ? tokens[4] : null, "Initial balance? (enter = 0)");
        // Fix 3: removed the pointless CardAddCommand round-trip
        _addCardHandler.Handle(name, currency, initialBalance ?? 0m);
    });

    private void HandleExpenseAddWizard(IReadOnlyList<string> tokens) => RunWizard(() =>
    {
        var inputs = CollectTransactionInputs(tokens);
        if (inputs == null) return;

        _addExpenseHandler.Handle(inputs.Amount, inputs.Category, inputs.CardId, inputs.Date, inputs.Note);

        var dailyReport = _dailyReportService.Generate(_clock.Today);
        _reportPrinter.Print(dailyReport);
    });

    private void HandleIncomeAddWizard(IReadOnlyList<string> tokens) => RunWizard(() =>
    {
        var inputs = CollectTransactionInputs(tokens);
        if (inputs == null) return;

        var sourceCardId = _addTransactionHandler.ResolveCardId(inputs.CardId);
        _addIncomeHandler.Handle(inputs.Amount, inputs.Category, sourceCardId, inputs.Date, inputs.Note);

        // Fix 4: cushion logic lives in its own class now
        _cushionTransferWizard.OfferTransfer(inputs.Amount, inputs.Category, sourceCardId, inputs.Date);

        var dailyReport = _dailyReportService.Generate(_clock.Today);
        _reportPrinter.Print(dailyReport);
    });

    private void HandleLimitSetWizard(IReadOnlyList<string> tokens) => RunWizard(() =>
    {
        var amount = _prompter.AskRequiredDecimal(tokens.Count >= 3 ? tokens[2] : null, "Daily limit amount?");
        _setDailyLimitHandler.Handle(amount);
    });
}