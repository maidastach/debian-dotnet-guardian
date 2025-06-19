namespace Guardian.Domain.Abstractions
{
    public sealed record Error(string Message, object? Data)
    {
        public static readonly Error None = new(string.Empty, default);
    }
}