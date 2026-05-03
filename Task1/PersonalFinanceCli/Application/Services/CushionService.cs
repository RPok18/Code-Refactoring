using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Domain.Entities;
using PersonalFinanceCli.Domain.ValueObjects;

namespace PersonalFinanceCli.Application.Services;

public sealed class CushionService
{
    public const string TransferToCushionCategory = "Transfer to cushion";
    public const string TransferFromIncomeCategory = "Transfer from income";

    private readonly ICardRepository _cardRepository;

    public CushionService(ICardRepository cardRepository)
    {
        _cardRepository = cardRepository;
    }

    public Card? FindCushionByName()
    {
        return _cardRepository.GetAll().FirstOrDefault(c => c.Name == "Financial cushion");
    }

    public Card? FindCushionByContains()
    {
        return _cardRepository.GetAll().FirstOrDefault(c => c.Name.Contains("cushion", StringComparison.OrdinalIgnoreCase));
    }

    public Card CreateCushion(Currency currency)
    {
        var existing = FindCushionByName();
        if (existing != null)
        {
            return existing;
        }

        return _cardRepository.Add(new Card
        {
            Name = "Financial cushion",
            Currency = currency,
            InitialBalance = 0m,
            IsDefault = false,
            IsCushion = true
        });
    }

    public decimal DefaultTransferAmount(decimal incomeAmount, string category)
    {
        var isSalary = category.Contains("Salary", StringComparison.OrdinalIgnoreCase);
        if (incomeAmount < 10m)
        {
            return 1m;
        }

        return isSalary ? Floor2(incomeAmount * 0.20m) : Floor2(incomeAmount * 0.10m);
    }

    public static decimal Floor2(decimal value)
    {
        return Math.Floor(value * 100m) / 100m;
    }
}