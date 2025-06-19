using Guardian.Domain.Abstractions;

namespace Guardian.Domain.Extensions
{
    public static class ResultExtension
    {
        public static TResult Match<TResult, T>(
            this Result<T> result,
            Func<T, TResult> onSuccess,
            Func<Error, TResult> onFailure)
        {
            return result.IsSuccess ? onSuccess(result.Data!) : onFailure(result.Error);
        }
    }
}