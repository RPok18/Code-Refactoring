using PersonalFinanceCli.Application.Repositories;
namespace PersonalFinanceCli.Infrastructure.Persistence;
public sealed class JsonOnboardingStateRepository : IOnboardingStateRepository
{
    private readonly JsonDataStore _dataStore;
    public JsonOnboardingStateRepository(JsonDataStore dataStore)
    {
        _dataStore = dataStore;
    }
    public DateOnly? GetLastCushionDeclinedDate()
    {
        return _dataStore.Load().LastCushionDeclinedDate;
    }
    public void SetLastCushionDeclinedDate(DateOnly? date)
    {
        var fileData = _dataStore.Load();
        fileData.LastCushionDeclinedDate = date;
        _dataStore.Save(fileData);
    }
    public bool HasSeenOnboarding()
    {
        return _dataStore.Load().HasSeenOnboarding;
    }
    public void SetHasSeenOnboarding(bool hasSeenOnboarding)
    {
        var fileData = _dataStore.Load();
        fileData.HasSeenOnboarding = hasSeenOnboarding;
        _dataStore.Save(fileData);
    }
}