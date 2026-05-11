using PersonalFinanceCli.Application.CommandHandlers;
using PersonalFinanceCli.Domain.Services;
using PersonalFinanceCli.Domain.ValueObjects;
using PersonalFinanceCli.Infrastructure.Time;
using PersonalFinanceCli.Presentation.Parsing;

namespace PersonalFinanceCli.Presentation.Rendering;

public sealed class CommandExecutor
{
    private readonly AddCardHandler _addCardHandler;
    private readonly SetDefaultCardHandler _setDefaultCardHandler;
    private readonly AddTransactionHandler _addTransactionHandler;
    private readonly AddIncomeHandler _addIncomeHandler;
    private readonly AddExpenseHandler _addExpenseHandler;
    private readonly SetDailyLimitHandler _setDailyLimitHandler;
    private readonly DailyReportService _dailyReportService;
    private readonly ReportPrinter _reportPrinter;
    private readonly ConsoleDisplay _consoleDisplay;
    private readonly IClock _clock;

    public CommandExecutor(
        AddCardHandler addCardHandler,
        SetDefaultCardHandler setDefaultCardHandler,
        AddTransactionHandler addTransactionHandler,
        AddIncomeHandler addIncomeHandler,
        AddExpenseHandler addExpenseHandler,
        SetDailyLimitHandler setDailyLimitHandler,
        DailyReportService dailyReportService,
        ReportPrinter reportPrinter,
        ConsoleDisplay consoleDisplay,
        IClock clock)
    {
        _addCardHandler = addCardHandler;
        _setDefaultCardHandler = setDefaultCardHandler;
        _addTransactionHandler = addTransactionHandler;
        _addIncomeHandler = addIncomeHandler;
        _addExpenseHandler = addExpenseHandler;
        _setDailyLimitHandler = setDailyLimitHandler;
        _dailyReportService = dailyReportService;
        _reportPrinter = reportPrinter;
        _consoleDisplay = consoleDisplay;
        _clock = clock;
    }

    public void ExecuteParsedCommand(ParsedCommand command)
    {
        var stateChanged = false;

        switch (command)
        {
            case CardAddCommand add:
                _addCardHandler.Handle(add.Name, add.Currency, add.InitialBalance);
                stateChanged = true;
                break;
            case CardListCommand:
                _consoleDisplay.PrintCards();
                break;
            case CardSetDefaultCommand setDefault:
                _setDefaultCardHandler.Handle(setDefault.CardId);
                stateChanged = true;
                break;
            case TransactionAddCommand trx:
                if (trx.Type == TransactionType.Income)
                {
                    _addIncomeHandler.Handle(trx.Amount, trx.Category, trx.CardId, trx.Date, trx.Note);
                }
                else
                {
                    _addExpenseHandler.Handle(trx.Amount, trx.Category, trx.CardId, trx.Date, trx.Note);
                }

                stateChanged = true;
                break;
            case LimitSetCommand setLimit:
                _setDailyLimitHandler.Handle(setLimit.Amount);
                stateChanged = true;
                break;
            case LimitShowCommand:
                _consoleDisplay.ShowLimit();
                break;
            case ReportDayCommand report:
                _reportPrinter.PrintDayUsingRepositories(report.Date ?? _clock.Today);
                break;
            default:
                throw new InvalidOperationException("Unknown parsed command.");
        }

        if (stateChanged)
        {
            var dailyReport = _dailyReportService.Generate(_clock.Today);
            _reportPrinter.Print(dailyReport);
        }
    }
}