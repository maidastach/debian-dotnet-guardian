namespace Guardian.Domain.Abstractions
{
    public class Result<T>
    {
        private Result(bool isSuccess, Error error, T? data)
        {
            if (IsInvalidSucces(isSuccess, error, data) || IsInvalidFailure(isSuccess, error, data))
            {
                throw new ArgumentException("Invalid Error", nameof(error));
            }

            IsSuccess = isSuccess;
            Error = error;
            Data = data;
        }
        public bool IsSuccess { get; }
        public Error Error { get; }
        public T? Data { get; }
        public static Result<T> Success(T data) => new(true, Error.None, data);
        public static Result<T> Failure(Error error) => new(false, error, default);
        private static bool IsInvalidSucces(bool isSuccess, Error error, T? data) => isSuccess && (error != Error.None || data == null);
        private static bool IsInvalidFailure(bool isSuccess, Error error, T? data) => !isSuccess && (error == Error.None || data != null);
    }
}