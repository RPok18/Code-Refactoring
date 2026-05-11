using PersonalFinanceCli.Presentation.Parsing;

namespace PersonalFinanceCli.Presentation.Rendering;

public sealed class ConsoleUI
{
    private readonly CommandParser _parser;
    private readonly IConsole _console;
    private readonly CommandExecutor _commandExecutor;
    private readonly InteractiveLoopRunner _loopRunner;

    public ConsoleUI(
        CommandParser parser,
        IConsole console,
        CommandExecutor commandExecutor,
        InteractiveLoopRunner loopRunner)
    {
        _parser = parser;
        _console = console;
        _commandExecutor = commandExecutor;
        _loopRunner = loopRunner;
    }

    public int Execute(string[] args)
    {
        try
        {
            var command = _parser.Parse(args);
            _commandExecutor.ExecuteParsedCommand(command);
            return 0;
        }
        catch (Exception ex)
        {
            _console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    public void RunInteractiveLoop() => _loopRunner.Run();
}