using PersonalFinanceCli.Application.CommandHandlers;
using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Application.Services;
using PersonalFinanceCli.Domain.ValueObjects;
using PersonalFinanceCli.Infrastructure.Time;

namespace PersonalFinanceCli.Presentation.Rendering;

public sealed class OnboardingManager
{
    private readonly InputPrompter _prompter;
    private readonly ICardRepository _cardRepository;
    private readonly IOnboardingStateRepository _onboardingStateRepository;
    private readonly AddTransactionHandler _addTransactionHandler;
    private readonly CushionService _cushionService;
    private readonly IClock _clock;
    private bool _onboardingChecked;

    public OnboardingManager(
        InputPrompter prompter,
        ICardRepository cardRepository,
        IOnboardingStateRepository onboardingStateRepository,
        AddTransactionHandler addTransactionHandler,
        CushionService cushionService,
        IClock clock)
    {
        _prompter = prompter;
        _cardRepository = cardRepository;
        _onboardingStateRepository = onboardingStateRepository;
        _addTransactionHandler = addTransactionHandler;
        _cushionService = cushionService;
        _clock = clock;
        _onboardingChecked = false;
    }

    public void EnsureOnboardingOnce()
    {
        if (_onboardingChecked)
        {
            return;
        }

        _onboardingChecked = true;

        var hasSeen = _onboardingStateRepository.HasSeenOnboarding();
        var cushion = _cushionService.FindCushionByName()
            ?? _addTransactionHandler.FindCushionCard()
            ?? _cushionService.FindCushionByContains();
        if (cushion != null)
        {
            _onboardingStateRepository.SetLastCushionDeclinedDate(null);
            _onboardingStateRepository.SetHasSeenOnboarding(true);
            return;
        }

        var cards = _cardRepository.GetAll();
        if (hasSeen && cards.Count == 0)
        {
            return;
        }

        var lastDeclined = _onboardingStateRepository.GetLastCushionDeclinedDate();
        if (lastDeclined.HasValue && _clock.Today < lastDeclined.Value.AddDays(14))
        {
            return;
        }

        if (_prompter.AskYesNoDefaultNo("Create 'Financial cushion' account? (y/n)"))
        {
            _cushionService.CreateCushion(Currency.RUB);
            _onboardingStateRepository.SetLastCushionDeclinedDate(null);
        }
        else
        {
            _onboardingStateRepository.SetLastCushionDeclinedDate(_clock.Today);
        }

        _onboardingStateRepository.SetHasSeenOnboarding(true);
    }
}