namespace PersonalFinanceCli.Infrastructure.Persistence;

public abstract class JsonRepositoryloadsave
{
    protected readonly JsonDataStore _dataStore;

    protected JsonRepositoryloadsave(JsonDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    protected void WithData(Action<FileData> action)
    {
        var fileData = _dataStore.Load();
        action(fileData);
        _dataStore.Save(fileData);
    }

    protected T WithData<T>(Func<FileData, T> action)
    {
        var fileData = _dataStore.Load();
        var result = action(fileData);
        _dataStore.Save(fileData);
        return result;
    }
}