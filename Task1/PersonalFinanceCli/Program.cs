using PersonalFinanceCli.Application.CommandHandlers;
using PersonalFinanceCli.Application.Services;
using PersonalFinanceCli.Domain.Services;
using PersonalFinanceCli.Infrastructure.Persistence;
using PersonalFinanceCli.Infrastructure.Time;
using PersonalFinanceCli.Presentation.Parsing;
using PersonalFinanceCli.Presentation.Rendering;

namespace PersonalFinanceCli;

public static class Program
{
    public static int Main(string[] args)
    {
      
        var systemConsole             = new SystemConsole();
        var fileDataPath              = Path.Combine(Directory.GetCurrentDirectory(), "fileData.json");
        var dataStore                 = new JsonDataStore(fileDataPath);
        var cardRepository            = new JsonCardRepository(dataStore);
        var transactionRepository     = new JsonTransactionRepository(dataStore);
        var limitRepository           = new JsonLimitRepository(dataStore);
        var onboardingStateRepository = new JsonOnboardingStateRepository(dataStore);
        var clock                     = new SystemClock();

     
        var validationHelper = new ValidationHelper(new IValidator[]
        {
            new AmountValidator(),
            new CategoryValidator()
        });

       
        var addCardHandler        = new AddCardHandler(cardRepository);
        var setDefaultCardHandler = new SetDefaultCardHandler(cardRepository);
        var cardResolver = new CardResolver(cardRepository);
        var cushionCardFinder = new CushionCardFinder(cardRepository);
        var addTransactionHandler = new AddTransactionHandler(transactionRepository, cardRepository, cardResolver, cushionCardFinder, validationHelper, clock);
        var addIncomeHandler      = new AddIncomeHandler(addTransactionHandler);
        var addExpenseHandler     = new AddExpenseHandler(transactionRepository, cardResolver, validationHelper, clock);
        var setDailyLimitHandler  = new SetDailyLimitHandler(limitRepository, cardRepository, clock);
        var dailyReportService    = new DailyReportService(cardRepository, transactionRepository, limitRepository);
        var cushionService        = new CushionService(cardRepository);

      
        var parser         = new CommandParser();
        var reportPrinter  = new ReportPrinter(systemConsole.Out, cardRepository, transactionRepository, limitRepository);
        var inputPrompter  = new InputPrompter(systemConsole, cardRepository);
        var consoleDisplay = new ConsoleDisplay(systemConsole, cardRepository, limitRepository, clock);

        var cushionTransferWizard = new CushionTransferWizard(
            inputPrompter,
            cardRepository,
            cushionCardFinder,
            cushionService,
            addTransactionHandler,
            systemConsole);

        var wizardCommandHandler = new WizardCommandHandler(
            inputPrompter,
            addCardHandler,
            addTransactionHandler,
            addIncomeHandler,
            addExpenseHandler,
            setDailyLimitHandler,
            dailyReportService,
            reportPrinter,
            clock,
            systemConsole,
            cushionTransferWizard,
            new WizardOptionCollector());

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
            clock);

        var onboardingManager = new OnboardingManager(
            inputPrompter,
            cardRepository,
            onboardingStateRepository,
            addTransactionHandler,
            cushionService,
            clock);

        var dispatcher = new CommandDispatcher(
            parser,
            systemConsole,
            wizardCommandHandler,
            commandExecutor,
            consoleDisplay);

        var loopRunner = new InteractiveLoopRunner(
            systemConsole,
            onboardingManager,
            dispatcher);

        var consoleUI = new ConsoleUI(
            parser,
            systemConsole,
            commandExecutor,
            loopRunner);

        if (args.Length > 0)
        {
            return consoleUI.Execute(args);
        }
        else
        {
            consoleUI.RunInteractiveLoop();
            return 0;
        }
    }
}