using PersonalFinanceCli.Application.Services;

public class ValidationHelper 
{
    private readonly IEnumerable<IValidator> _validators;

    public ValidationHelper(IEnumerable<IValidator> validators)
    {
        _validators = validators;
    }

    public void ValidateAmount(decimal amount)
    {
        foreach (var validator in _validators)
        {
            validator.Validate(amount);
        }
    }

    public void ValidateCategory(string category)
    {
        foreach (var validator in _validators)
        {
            validator.Validate(category);
        }
    }
}