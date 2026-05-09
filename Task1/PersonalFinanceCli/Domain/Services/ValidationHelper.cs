namespace PersonalFinanceCli.Application.Services;

public static class ValidationHelper
{
    public static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new InvalidOperationException("Amount must be > 0.");
        }
    }

    public static void ValidateCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            throw new InvalidOperationException("Category cannot be empty.");
        }
    }
}