// InteractiveLoopRunner.cs
namespace PersonalFinanceCli.Presentation.Rendering;

public sealed class InteractiveLoopRunner
{
    private readonly IConsole _console;
    private readonly OnboardingManager _onboardingManager;
    private readonly CommandDispatcher _dispatcher;

    public InteractiveLoopRunner(
        IConsole console,
        OnboardingManager onboardingManager,
        CommandDispatcher dispatcher)
    {
        _console = console;
        _onboardingManager = onboardingManager;
        _dispatcher = dispatcher;
    }

    public void Run()
    {
        _onboardingManager.EnsureOnboardingOnce();

        while (true)
        {
            _console.Write("> ");
            var line = _console.ReadLine();

            if (line is null)                                         
               return;
            if (string.IsNullOrWhiteSpace(line))                      
              continue;
            if (line.Equals("exit", StringComparison.OrdinalIgnoreCase)) 
            return;

            _dispatcher.Dispatch(line);
        }
    }
}