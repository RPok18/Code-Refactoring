using PersonalFinanceCli.Presentation.Parsing;           
namespace PersonalFinanceCli.Presentation.Rendering;
public sealed class CommandDispatcher
{
    private readonly CommandParser _parser;
    private readonly IConsole _console;
    private readonly WizardCommandHandler _wizardCommandHandler;
    private readonly CommandExecutor _commandExecutor;
    private readonly ConsoleDisplay _consoleDisplay;
    public CommandDispatcher(
        CommandParser parser,
        IConsole console,
        WizardCommandHandler wizardCommandHandler,
        CommandExecutor commandExecutor,
        ConsoleDisplay consoleDisplay)
    {
        _parser = parser;
        _console = console;
        _wizardCommandHandler = wizardCommandHandler;
        _commandExecutor = commandExecutor;
        _consoleDisplay = consoleDisplay;
    }
    public void Dispatch(string line)
    {
        if (line.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            _consoleDisplay.PrintHelp();
            return;
        }
        if (_wizardCommandHandler.TryHandleWizard(line))
            return;
        try
        {
            var parsed = _parser.Parse(line);
            _commandExecutor.ExecuteParsedCommand(parsed);
        }
        catch (Exception ex)
        {
            _console.WriteLine($"Error: {ex.Message}");
            _console.WriteLine("type help");
        }
    }
}