namespace OrderFlow.Domain.Common
{
    public class Result
    {
        public bool IsSuccess { get; private set; }
        public string? Field { get; }
        public string? Error { get; private set; }

        protected Result(bool isSuccess, string? field, string? error)
        {
            if (isSuccess && !string.IsNullOrWhiteSpace(error))
                throw new InvalidOperationException("Successful result cannot contain an error message.");

            if (!isSuccess && string.IsNullOrWhiteSpace(error))
                throw new ArgumentNullException(nameof(error), "Error is required for a failed result.");

            IsSuccess = isSuccess;
            Field = field;
            Error = error;
        }

        public static Result Success() => new Result(true, null, null);

        public static Result Failure(string error) => new Result(false, null, error);

        public static Result Failure(string field, string error) => new Result(false, field, error);
    }

    public class Result<T> : Result
    {
        private readonly T _value;

        public T Value
        {
            get
            {
                if (!IsSuccess)
                    throw new InvalidOperationException("Cannot access Value when result is a failure.");

                return _value;
            }
        }

        protected Result(T value) : base(true, null, null)
        {
            _value = value;
        }

        protected Result(string error) : base(false, null, error)
        {
            _value = default!;
        }

        protected Result(string? field, string error) : base(false, field, error)
        {
            _value = default!;
        }

        public static Result<T> Success(T value) => new Result<T>(value);

        public static new Result<T> Failure(string error) => new Result<T>(error);

        public static Result<T> Failure(string field, string error) => new Result<T>(field, error);
    }
}

