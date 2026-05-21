using PersonalFinanceCli.Application.Services;

public class AmountValidator : IValidator
{
    public void Validate(object value)
    {
        if (value is decimal amount && amount <= 0)
        {
            throw new InvalidOperationException("Amount must be > 0.");
        }
    }
}