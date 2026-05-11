using PersonalFinanceCli.Application.Services;

public class CategoryValidator : IValidator
{
    public void Validate(object value)
    {
        if (value is string category && string.IsNullOrWhiteSpace(category))
        {
            throw new InvalidOperationException("Category cannot be empty.");
        }
    }
}