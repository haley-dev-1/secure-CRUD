namespace EdgeAdmin.Business.Results;

public sealed class ServiceError
{
    public string Code { get; }
    public string Message { get; }

    public ServiceError(string code, string message)
    {
        Code = code;
        Message = message;
    }
}
