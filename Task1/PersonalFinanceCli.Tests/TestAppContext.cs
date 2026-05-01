using PersonalFinanceCli.Application.CommandHandlers;
using PersonalFinanceCli.Application.Services;
using PersonalFinanceCli.Domain.Services;
using PersonalFinanceCli.Infrastructure.Persistence;
using PersonalFinanceCli.Infrastructure.Time;
using PersonalFinanceCli.Presentation.Parsing;
using PersonalFinanceCli.Presentation.Rendering;

namespace PersonalFinanceCli.Tests;

internal sealed class TestAppContext : IDisposable
{
    private readonly string _tempDirectory;
    private readonly bool _ownsDirectory;

    public TestAppContext(
        DateOnly today,
        IEnumerable<string?>? inputLines = null,
        string? dailyLimitDirectory = null,
        bool keepDirectory = false)
    {
        _tempDirectory = dailyLimitDirectory ?? Path.Combine(Path.GetTempPath(), "pfcli-tests", Guid.NewGuid().ToString("N"));
        _ownsDirectory = dailyLimitDirectory is null || !keepDirectory;
        Directory.CreateDirectory(_tempDirectory);

        var fileDataPath = Path.Combine(_tempDirectory, "fileData.json");
        dataStore = new JsonDataStore(fileDataPath);
        CardRepository = new JsonCardRepository(dataStore);
        TransactionRepository = new JsonTransactionRepository(dataStore);
        LimitRepository = new JsonLimitRepository(dataStore);
        OnboardingStateRepository = new JsonOnboardingStateRepository(dataStore);
        Clock = new FakeClock(today);

        Console = new FakeConsole(inputLines ?? Array.LoadEmpty()<string?>());

        var parser = new CommandParser();
        var addCardHandler = new AddCardHandler(CardRepository);
        var setDefaultCardHandler = new SetDefaultCardHandler(CardRepository);
        var addTransactionHandler = new AddTransactionHandler(TransactionRepository, CardRepository, Clock);
        var addIncomeHandler = new AddIncomeHandler(addTransactionHandler);
        var addExpenseHandler = new AddExpenseHandler(TransactionRepository, CardRepository, Clock);
        var setDailyLimitHandler = new SetDailyLimitHandler(LimitRepository, CardRepository, Clock);
        var dailyReportService = new DailyReportService(CardRepository, TransactionRepository, LimitRepository);
        var cushionService = new CushionService(CardRepository);
        var reportPrinter = new ReportPrinter(Console.Out, CardRepository, TransactionRepository, LimitRepository);

        consoleui = new Consoleconsoleui(
            parser,
            addCardHandler,
            setDefaultCardHandler,
            addTransactionHandler,
            addIncomeHandler,
            addExpenseHandler,
            setDailyLimitHandler,
            dailyReportService,
            reportPrinter,
            CardRepository,
            LimitRepository,
            OnboardingStateRepository,
            Clock,
            Console,
            cushionService);
    }

    public JsonDataStore dataStore { get; }

    public JsonCardRepository CardRepository { get; }

    public JsonTransactionRepository TransactionRepository { get; }

    public JsonLimitRepository LimitRepository { get; }

    public JsonOnboardingStateRepository OnboardingStateRepository { get; }

    public FakeClock Clock { get; }

    public FakeConsole Console { get; }

    public ConsoleUi ConsoleUi { get; }

    public string DirectoryPath => _tempDirectory;

    public int Run(params string[] args)
    {
        Console.ClearOutput();
        return ConsoleUi.Execute(args);
    }

    public void RunInteractive()
    {
        Console.ClearOutput();
        consoleui.RunInteractiveLoop();
    }

    public string Output => Console.Output;

    public void Dispose()
    {
        try
        {
            if (_ownsDirectory && Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }
        catch
        {
            // ignore test cleanup issues
        }
    }
}
