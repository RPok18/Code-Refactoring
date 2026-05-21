using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Application.Services;
using PersonalFinanceCli.Domain.Entities;
using PersonalFinanceCli.Domain.ValueObjects;
using PersonalFinanceCli.Infrastructure.Time;

namespace PersonalFinanceCli.Application.CommandHandlers;

public sealed class AddExpenseHandler
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICardResolver _cardResolver;
    private readonly ValidationHelper _validationHelper;
    private readonly IClock _clock;

    public AddExpenseHandler(
        ITransactionRepository transactionRepository,
        ICardResolver cardResolver,
        ValidationHelper validationHelper,
        IClock clock)
    {
        _transactionRepository = transactionRepository;
        _cardResolver = cardResolver;
        _validationHelper = validationHelper;
        _clock = clock;
    }

    public Transaction Handle(decimal amount, string category, int? cardId, DateOnly? date, string? note)
    {
        _validationHelper.ValidateAmount(amount);
        _validationHelper.ValidateCategory(category);

        var resolvedCardId = _cardResolver.ResolveCardId(cardId, TransactionType.Expense);

        var trx = new Transaction
        {
            CardId = resolvedCardId,
            Amount = amount,
            Category = category,
            Date = date ?? _clock.Today,
            Note = note,
            Type = TransactionType.Expense
        };

        return _transactionRepository.Add(trx);
    }
}