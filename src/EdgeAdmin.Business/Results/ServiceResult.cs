namespace EdgeAdmin.Business.Results;

public sealed class ServiceResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public ServiceError? Error { get; }

    private ServiceResult(bool isSuccess, T? value, ServiceError? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static ServiceResult<T> Success(T value) => new(true, value, null);
    public static ServiceResult<T> Failure(string code, string message) => new(false, default, new ServiceError(code, message));
}
