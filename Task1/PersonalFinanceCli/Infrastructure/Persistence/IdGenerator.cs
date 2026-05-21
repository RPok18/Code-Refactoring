namespace PersonalFinanceCli.Infrastructure.Persistence;

internal static class RepositoryIdGenerator
{
    public static int NextId<T>(IEnumerable<T> items, Func<T, int> idSelector)
    {
        var list = items.ToList();
        return list.Count == 0 ? 1 : list.Max(idSelector) + 1;
    }
}