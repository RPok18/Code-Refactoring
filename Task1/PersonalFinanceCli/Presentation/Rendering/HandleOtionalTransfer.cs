public sealed class CushionTransferWizard
{
    private readonly InputPrompter _prompter;
    private readonly ICardRepository _cardRepository;
    private readonly CushionCardFinder _cushionCardFinder;
    private readonly CushionService _cushionService;
    private readonly AddTransactionHandler _addTransactionHandler;
    private readonly IConsole _console;

    public CushionTransferWizard(
        InputPrompter prompter,
        ICardRepository cardRepository,
        CushionCardFinder cushionCardFinder,
        CushionService cushionService,
        AddTransactionHandler addTransactionHandler,
        IConsole console)
    {
        _prompter = prompter;
        _cardRepository = cardRepository;
        _cushionCardFinder = cushionCardFinder;
        _cushionService = cushionService;
        _addTransactionHandler = addTransactionHandler;
        _console = console;
    }

    public void OfferTransfer(decimal incomeAmount, string category, int sourceCardId, DateOnly? date)
    {
        if (!_prompter.AskYesNoDefaultYes("Transfer part of income to 'Financial cushion'? (y/n)"))
            return;

        var sourceCard = _cardRepository.GetById(sourceCardId);
        if (sourceCard == null) return;

        var cushion = _cushionCardFinder.FindCushion();
        if (cushion == null)
        {
            if (_prompter.AskYesNo("Cushion account not found. Create now? (y/n)"))
                cushion = _cushionService.CreateCushion(sourceCard.Currency);
            else
                return;
        }

        if (sourceCard.Currency != cushion.Currency)
        {
            if (!_prompter.AskYesNoWithCancel("Currencies do not match. Transfer anyway? (y/n)", out _))
                return;
        }

        var transferAmount = _prompter.AskTransferAmount(incomeAmount, category, _cushionService);
        if (!transferAmount.HasValue)
        {
            _console.WriteLine("Transfer cancelled.");
            return;
        }

        _addTransactionHandler.AddTransferPair(sourceCardId, cushion.Id, transferAmount.Value, date);
    }
}