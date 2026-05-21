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
    private readonly ConsoleUI _consoleUI;

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
        DataStore = new JsonDataStore(fileDataPath);
        CardRepository = new JsonCardRepository(DataStore);
        TransactionRepository = new JsonTransactionRepository(DataStore);
        LimitRepository = new JsonLimitRepository(DataStore);
        OnboardingStateRepository = new JsonOnboardingStateRepository(DataStore);
        Clock = new FakeClock(today);
        Console = new FakeConsole(inputLines ?? Array.Empty<string?>());

        var parser = new CommandParser();
        var addCardHandler = new AddCardHandler(CardRepository);
        var setDefaultCardHandler = new SetDefaultCardHandler(CardRepository);
        var cardResolver = new CardResolver(CardRepository);
        var cushionCardFinder = new CushionCardFinder(CardRepository);
        var validationHelper = new ValidationHelper(new IValidator[] { new AmountValidator(), new CategoryValidator() });
        var addTransactionHandler = new AddTransactionHandler(TransactionRepository, CardRepository, cardResolver, cushionCardFinder, validationHelper, Clock);
        var addIncomeHandler = new AddIncomeHandler(addTransactionHandler);
        var addExpenseHandler = new AddExpenseHandler(TransactionRepository, cardResolver, validationHelper, Clock);
        var setDailyLimitHandler = new SetDailyLimitHandler(LimitRepository, CardRepository, Clock);
        var dailyReportService = new DailyReportService(CardRepository, TransactionRepository, LimitRepository);
        var cushionService = new CushionService(CardRepository);
        var reportPrinter = new ReportPrinter(Console.Out, CardRepository, TransactionRepository, LimitRepository);

        var consoleDisplay = new ConsoleDisplay(Console, CardRepository, LimitRepository, Clock);
        var commandExecutor = new CommandExecutor(
            addCardHandler,
            setDefaultCardHandler,
            addTransactionHandler,
            addIncomeHandler,
            addExpenseHandler,
            setDailyLimitHandler,
            dailyReportService,
            reportPrinter,
            consoleDisplay,
            Clock);

        var inputPrompter = new InputPrompter(Console, CardRepository); // or create a fake one

        var onboardingManager = new OnboardingManager(
            inputPrompter,
            CardRepository,
            OnboardingStateRepository,
            addTransactionHandler,
            cushionService,
            Clock);

        var cushionTransferWizard = new CushionTransferWizard(
            inputPrompter,
            CardRepository,
            cushionCardFinder,
            cushionService,
            addTransactionHandler,
            Console);

        var wizardCommandHandler = new WizardCommandHandler(
            inputPrompter,
            addCardHandler,
            addTransactionHandler,
            addIncomeHandler,
            addExpenseHandler,
            setDailyLimitHandler,
            dailyReportService,
            reportPrinter,
            Clock,
            Console,
            cushionTransferWizard,
            new WizardOptionCollector());

        var dispatcher = new CommandDispatcher(
            parser,
            Console,
            wizardCommandHandler,
            commandExecutor,
            consoleDisplay);

        var loopRunner = new InteractiveLoopRunner(
            Console,
            onboardingManager,
            dispatcher);

        _consoleUI = new ConsoleUI(
            parser,
            Console,
            commandExecutor,
            loopRunner);
    }

    public JsonDataStore DataStore { get; }
    public JsonCardRepository CardRepository { get; }
    public JsonTransactionRepository TransactionRepository { get; }
    public JsonLimitRepository LimitRepository { get; }
    public JsonOnboardingStateRepository OnboardingStateRepository { get; }
    public FakeClock Clock { get; }
    public FakeConsole Console { get; }
    public string DirectoryPath => _tempDirectory;

    public int Run(params string[] args)
    {
        Console.ClearOutput();
        return _consoleUI.Execute(args);
    }

    public void RunInteractive()
    {
        Console.ClearOutput();
        _consoleUI.RunInteractiveLoop();
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