using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Application.Services;  // Add this
using PersonalFinanceCli.Domain.Entities;
using PersonalFinanceCli.Domain.ValueObjects;
using PersonalFinanceCli.Infrastructure.Time;

namespace PersonalFinanceCli.Application.CommandHandlers;

public sealed class AddExpenseHandler
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly CardResolver _cardResolver;  // Add this
    private readonly IClock _clock;

    public AddExpenseHandler(
        ITransactionRepository transactionRepository,
        ICardRepository cardRepository,  // Keep for CardResolver
        IClock clock)
    {
        _transactionRepository = transactionRepository;
        _cardResolver = new CardResolver(cardRepository);  // Or inject via DI
        _clock = clock;
    }

    public Transaction Handle(decimal amount, string category, int? cardId, DateOnly? date, string? note)
    {
        ValidationHelper.ValidateAmount(amount);  // Replace inline check
        ValidationHelper.ValidateCategory(category);  // Replace inline check

        var resolvedCardId = _cardResolver.ResolveCardId(cardId, TransactionType.Expense);  // Replace inline logic

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