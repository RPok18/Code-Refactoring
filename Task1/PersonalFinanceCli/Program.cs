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
        // main method starts with app
        var systemconsole = new SystemConsole();
        // json file path 
        var fileDataPath = Path.Combine(Directory.GetCurrentDirectory(), "fileData.json");

        // repositories 
        var dataStore = new JsonDataStore(fileDataPath);
        var cardRepository = new JsonCardRepository(dataStore);
        var transactionRepository = new JsonTransactionRepository(dataStore);
        var limitRepository = new JsonLimitRepository(dataStore);
        var onboardingStateRepository = new JsonOnboardingStateRepository(dataStore);
        var clock = new SystemClock();

        var parser = new CommandParser();
        var addCardHandler = new AddCardHandler(cardRepository);
        var setDefaultCardHandler = new SetDefaultCardHandler(cardRepository);
        var addTransactionHandler = new AddTransactionHandler(transactionRepository, cardRepository, clock);
        var addIncomeHandler = new AddIncomeHandler(addTransactionHandler);
        var addExpenseHandler = new AddExpenseHandler(transactionRepository, cardRepository, clock);
        var setDailyLimitHandler = new SetDailyLimitHandler(limitRepository, cardRepository, clock);
        var dailyReportService = new DailyReportService(cardRepository, transactionRepository, limitRepository);
        var cushionService = new CushionService(cardRepository);
        var reportPrinter = new ReportPrinter(systemconsole.Out, cardRepository, transactionRepository, limitRepository);

        // ConsoleUI creation 
        var ConsoleUI = new ConsoleUI(
            parser,
            addCardHandler,
            setDefaultCardHandler,
            addTransactionHandler,
            addIncomeHandler,
            addExpenseHandler,
            setDailyLimitHandler,
            dailyReportService,
            reportPrinter,
            cardRepository,
            limitRepository,
            onboardingStateRepository,
            clock,
            console,
            cushionService);

        // Running command or exit
        if (args.Length > 0)
        {
            return ConsoleUI.Execute(args);
        }

        
        consoleui.RunInteractiveLoop();
       
        return 0;
    }
}
