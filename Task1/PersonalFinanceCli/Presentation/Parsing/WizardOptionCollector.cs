using System.Text.RegularExpressions;

namespace PersonalFinanceCli.Presentation.Parsing;

public sealed class WizardOptionCollector
{
    private static readonly Regex StrictDateRegex = new(@"^\d{4}-\d{2}-\d{2}$", RegexOptions.Compiled);

    public WizardOptions Collect(IReadOnlyList<string> tokens, int startIndex)
    {
        string? rawCardId = null;
        DateOnly? date = null;
        string? note = null;

        var i = startIndex;
        while (i < tokens.Count)
        {
            var option = tokens[i];
            if (option == "--card")
            {
                i++;
                rawCardId = i < tokens.Count ? tokens[i] : null;
                if (string.IsNullOrWhiteSpace(rawCardId))
                {
                    return new WizardOptions(null, null, null, "Invalid --card value.");
                }
            }
            else if (option == "--date")
            {
                i++;
                var rawDate = i < tokens.Count ? tokens[i] : null;
                if (string.IsNullOrWhiteSpace(rawDate) || !StrictDateRegex.IsMatch(rawDate) || !DateOnly.TryParse(rawDate, out var parsedDate))
                {
                    return new WizardOptions(null, null, null, "Invalid --date value. Use strict YYYY-MM-DD.");
                }

                date = parsedDate;
            }
            else if (option == "--note")
            {
                i++;
                if (i >= tokens.Count)
                {
                    return new WizardOptions(null, null, null, "Invalid --note value.");
                }

                note = tokens[i];
            }
            else
            {
                return new WizardOptions(null, null, null, $"Unknown option {option}.");
            }

            i++;
        }

        return new WizardOptions(rawCardId, date, note, null);
    }
}

public readonly record struct WizardOptions(string? RawCardId, DateOnly? Date, string? Note, string? Error);