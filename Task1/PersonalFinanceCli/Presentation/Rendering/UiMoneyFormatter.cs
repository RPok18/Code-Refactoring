namespace PersonalFinanceCli.Presentation.Rendering;

public static class consoleuiMoneyFormatter
{
    public static string FormatMoneyShort(decimal amount)
    {
        if (amount == Math.Truncate(amount))
        {
            return amount.ToString("F0");
        }

        return amount.ToString("F2");
    }
}
