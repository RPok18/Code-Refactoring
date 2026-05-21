using PersonalFinanceCli.Application.Repositories;

namespace PersonalFinanceCli.Infrastructure.Persistence;

public sealed class JsonOnboardingStateRepository : JsonRepositoryloadsave, IOnboardingStateRepository
{
    private readonly JsonDataStore _dataStore;

    public JsonOnboardingStateRepository(JsonDataStore dataStore)
        : base(dataStore)
    {
        _dataStore = dataStore;
    }

    public DateOnly? GetLastCushionDeclinedDate()
    {
        return _dataStore.Load().LastCushionDeclinedDate;
    }

   public void SetLastCushionDeclinedDate(DateOnly? date)
{
    WithData(fileData => fileData.LastCushionDeclinedDate = date);
}

    public bool HasSeenOnboarding()
    {
        return _dataStore.Load().HasSeenOnboarding;
    }

   public void SetHasSeenOnboarding(bool hasSeenOnboarding)
{
    WithData(fileData => fileData.HasSeenOnboarding = hasSeenOnboarding);
}
}