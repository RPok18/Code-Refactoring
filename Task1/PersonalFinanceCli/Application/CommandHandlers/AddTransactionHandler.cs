using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Application.Services;
using PersonalFinanceCli.Domain.Entities;
using PersonalFinanceCli.Domain.ValueObjects;
using PersonalFinanceCli.Infrastructure.Time;

namespace PersonalFinanceCli.Application.CommandHandlers;

public sealed class AddTransactionHandler
{
    public const string TransferToCushion = "Transfer to cushion";
    public const string TransferFromIncome = "Transfer from income";

    private readonly ITransactionRepository _transactionRepository;
    private readonly ICardRepository _cardRepository;
    private readonly ICardResolver _cardResolver;
    private readonly ICushionCardFinder _cushionCardFinder;
    private readonly ValidationHelper _validationHelper;
    private readonly IClock _clock;

    public AddTransactionHandler(
        ITransactionRepository transactionRepository,
        ICardRepository cardRepository,
        ICardResolver cardResolver,
        ICushionCardFinder cushionCardFinder,
        ValidationHelper validationHelper,
        IClock clock)
    {
        _transactionRepository = transactionRepository;
        _cardRepository = cardRepository;
        _cardResolver = cardResolver;
        _cushionCardFinder = cushionCardFinder;
        _validationHelper = validationHelper;
        _clock = clock;
    }

    public Transaction Handle(
        TransactionType type,
        decimal amount,
        string category,
        int? cardId,
        DateOnly? date,
        string? note)
    {
        _validationHelper.ValidateAmount(amount);
        _validationHelper.ValidateCategory(category);

        var resolvedCardId = _cardResolver.ResolveCardId(cardId, type);
        var card = _cardRepository.GetById(resolvedCardId);
        if (card is null)
        {
            throw new InvalidOperationException("Card not found.");
        }

        var trx = new Transaction
        {
            CardId = resolvedCardId,
            Amount = amount,
            Category = category,
            Date = date ?? _clock.Today,
            Note = note,
            Type = type
        };

        return _transactionRepository.Add(trx);
    }

    public int ResolveCardId(int? cardId)
    {
        return _cardResolver.ResolveCardId(cardId, TransactionType.Income);
    }

    public Card? FindCushionCard()
    {
        return _cushionCardFinder.FindCushion();
    }

    public void AddTransferPair(int fromCardId, int cushionCardId, decimal amount, DateOnly? date)
    {
        var transferDate = date ?? _clock.Today;

        _transactionRepository.Add(new Transaction
        {
            CardId = fromCardId,
            Amount = amount,
            Category = TransferToCushion,
            Date = transferDate,
            Note = "auto",
            Type = TransactionType.Expense
        });

        _transactionRepository.Add(new Transaction
        {
            CardId = cushionCardId,
            Amount = amount,
            Category = TransferFromIncome,
            Date = transferDate,
            Note = "auto",
            Type = TransactionType.Income
        });
    }
}