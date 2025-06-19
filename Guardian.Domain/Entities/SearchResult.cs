namespace Guardian.Domain.Entities
{
    public class SearchResult<T>(IEnumerable<T> data, int count)
    {
        public int Count { get; } = count;
        public IEnumerable<T> Data { get; } = data;
    }
}