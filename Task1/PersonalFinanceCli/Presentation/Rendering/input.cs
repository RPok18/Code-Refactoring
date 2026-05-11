using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Application.Services;
using PersonalFinanceCli.Domain.ValueObjects;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PersonalFinanceCli.Presentation.Rendering;

public sealed class InputPrompter
{
    private readonly IConsole _console;
    private readonly ICardRepository _cardRepository;

    public InputPrompter(IConsole console, ICardRepository cardRepository)
    {
        _console = console;
        _cardRepository = cardRepository;
    }

    public string AskRequiredText(string? preset, string prompt)
    {
        var current = preset;
        while (true)
        {
            if (current == null)
            {
                _console.Write($"{prompt} ");
                current = ReadWizardAnswer();
            }

            if (string.Equals(current, "cancel", StringComparison.OrdinalIgnoreCase))
            {
                throw new WizardCancelledException();
            }

            if (!string.IsNullOrWhiteSpace(current))
            {
                return current;
            }

            _console.WriteLine("Error: Value is required.");
            current = null;
        }
    }

    public decimal AskRequiredDecimal(string? preset, string prompt)
    {
        var current = preset;
        while (true)
        {
            if (current == null)
            {
                _console.Write($"{prompt} ");
                current = ReadWizardAnswer();
            }

            if (string.Equals(current, "cancel", StringComparison.OrdinalIgnoreCase))
            {
                throw new WizardCancelledException();
            }

            if (TryParseFlexibleDecimal(current, out var value))
            {
                return value;
            }

            _console.WriteLine("Error: Invalid decimal.");
            current = null;
        }
    }

    public decimal? AskOptionalDecimal(string? preset, string prompt)
    {
        var current = preset;
        while (true)
        {
            if (current == null)
            {
                _console.Write($"{prompt} ");
                current = ReadWizardAnswer();
            }

            if (string.Equals(current, "cancel", StringComparison.OrdinalIgnoreCase))
            {
                throw new WizardCancelledException();
            }

            if (string.IsNullOrWhiteSpace(current))
            {
                return null;
            }

            if (TryParseFlexibleDecimal(current, out var value))
            {
                return value;
            }

            _console.WriteLine("Error: Invalid decimal.");
            current = null;
        }
    }

    public DateOnly? AskOptionalDate(string? preset, string prompt)
    {
        var current = preset;
        while (true)
        {
            if (current == null)
            {
                _console.Write($"{prompt} ");
                current = ReadWizardAnswer();
            }

            if (string.Equals(current, "cancel", StringComparison.OrdinalIgnoreCase))
            {
                throw new WizardCancelledException();
            }

            if (string.IsNullOrWhiteSpace(current))
            {
                return null;
            }

            if (DateOnly.TryParse(current, out var value))
            {
                return value;
            }

            _console.WriteLine("Error: Invalid date.");
            current = null;
        }
    }

    public string AskCurrency(string? preset)
    {
        var current = preset;
        while (true)
        {
            if (current == null)
            {
                _console.Write("Currency (RUB/EUR)? ");
                current = ReadWizardAnswer();
            }

            if (string.Equals(current, "cancel", StringComparison.OrdinalIgnoreCase))
            {
                throw new WizardCancelledException();
            }

            if (Enum.TryParse<Currency>(current, true, out _))
            {
                return current;
            }

            _console.WriteLine("Error: Unknown currency. Allowed: RUB, EUR.");
            current = null;
        }
    }

    public int? ResolveCardWizard(string? preset, string prompt)
    {
        var current = preset;
        while (true)
        {
            if (current == null)
            {
                _console.Write($"{prompt} ");
                current = ReadWizardAnswer();
            }

            if (string.Equals(current, "cancel", StringComparison.OrdinalIgnoreCase))
            {
                throw new WizardCancelledException();
            }

            if (string.IsNullOrWhiteSpace(current))
            {
                return null;
            }

            if (int.TryParse(current.Trim(), out var parsed))
            {
                return parsed;
            }

            if (Regex.IsMatch(current, "^[0-9a-fA-F-]{36}$") && Guid.TryParse(current, out var guid))
            {
                var tail = guid.ToString("N")[20..];
                if (int.TryParse(tail, out var fromGuid))
                {
                    return fromGuid;
                }
            }

            var cards = _cardRepository.GetAll();
            var byExact = cards.FirstOrDefault(c => c.Name.Equals(current.Trim(), StringComparison.OrdinalIgnoreCase));
            if (byExact != null)
            {
                return byExact.Id;
            }

            var byContains = cards.Where(c => c.Name.Contains(current, StringComparison.OrdinalIgnoreCase)).ToList();
            if (byContains.Count == 1)
            {
                return byContains[0].Id;
            }

            _console.WriteLine("Error: Invalid card. Enter card id or card name.");
            current = null;
        }
    }

    public bool AskYesNo(string prompt, bool? defaultAnswer = null)
    {
        while (true)
        {
            _console.Write($"{prompt} ");
            var input = _console.ReadLine();
            if (input == null) return false;

            var value = input.Trim();
            if (value.Length == 0 && defaultAnswer.HasValue) return defaultAnswer.Value;
            if (value.Equals("y", StringComparison.OrdinalIgnoreCase)) return true;
            if (value.Equals("n", StringComparison.OrdinalIgnoreCase)) return false;

            _console.WriteLine("Error: Please answer y/n.");
        }
    }

    public bool AskYesNoDefaultYes(string prompt) => AskYesNo(prompt, true);

    public bool AskYesNoDefaultNo(string prompt) => AskYesNo(prompt, false);

    public bool AskYesNoWithCancel(string prompt, out bool wasCanceled)
    {
        wasCanceled = false;
        while (true)
        {
            _console.Write($"{prompt} ");
            var input = _console.ReadLine();
            if (input == null)
            {
                wasCanceled = true;
                return false;
            }

            var value = input.Trim();
            if (value.Equals("y", StringComparison.OrdinalIgnoreCase)) return true;
            if (value.Equals("n", StringComparison.OrdinalIgnoreCase)) return false;
            if (value.Equals("cancel", StringComparison.OrdinalIgnoreCase))
            {
                wasCanceled = true;
                return false;
            }

            _console.WriteLine("Error: Please answer y/n or cancel.");
        }
    }

    /// <summary>
    /// Asks user for transfer amount with multiple input formats:
    /// - Empty: uses default amount from CushionService
    /// - Percentage (e.g., "25%"): calculates percentage of income
    /// - Absolute amount: uses directly
    /// </summary>
    public decimal? AskTransferAmount(decimal incomeAmount, string category, CushionService cushionService)
    {
        while (true)
        {
            _console.Write("How much to transfer? (enter = default / percent like 25% or absolute amount) ");
            var input = _console.ReadLine();
            if (input == null || input.Equals("cancel", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            decimal amount;
            if (string.IsNullOrWhiteSpace(input))
            {
                // Use CushionService to calculate default
                amount = cushionService.DefaultTransferAmount(incomeAmount, category);
            }
            else if (input.TrimEnd().EndsWith("%", StringComparison.Ordinal))
            {
                var rawPercent = input.Trim()[..^1];
                if (!TryParseFlexibleDecimal(rawPercent, out var percent))
                {
                    _console.WriteLine("Error: Invalid transfer amount.");
                    continue;
                }

                amount = CushionService.Floor2(incomeAmount * percent / 100m);
            }
            else
            {
                if (!decimal.TryParse(input.Trim(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var explicitAmount))
                {
                    _console.WriteLine("Error: Invalid transfer amount.");
                    continue;
                }

                amount = Math.Round(explicitAmount, 2, MidpointRounding.AwayFromZero);
            }

            if (amount <= 0m || amount > incomeAmount)
            {
                var formatter = new UiMoneyFormatter();
                _console.WriteLine($"Error: Transfer amount must be > 0 and <= income ({formatter.FormatMoneyShort(incomeAmount)} max).");
                continue;
            }

            return amount;
        }
    }

    private string ReadWizardAnswer()
    {
        var answer = _console.ReadLine();
        if (answer == null)
        {
            throw new WizardCancelledException();
        }

        return answer;
    }

    private static bool TryParseFlexibleDecimal(string input, out decimal value)
    {
        var normalized = input.Trim().Replace(',', '.');
        return decimal.TryParse(
            normalized,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out value);
    }
}

public sealed class WizardCancelledException : Exception;