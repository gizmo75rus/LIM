namespace LIM.WebApp.Models;

public class AppResponce
{
    private readonly bool _success;
    private readonly IEnumerable<AppError>? _errors;
    private readonly string? _message;

    public AppResponce()
    {
        _success = true;
        _message = null;
        _errors = null;

    }
    
    public AppResponce(string message)
    {
        _success = true;
        _message = message;
    }

    public AppResponce(IEnumerable<AppError> errors)
    {
        _success = false;
        _errors = errors;
        _message = null;
    }

    public AppResponce(AppError error)
    {
        _success = false;
        _message = null;
        _errors = new AppError[] { error };
    }

    public AppResponce(string message, bool success)
    {
        _success = success;
        _message = message;
    }

        
    public bool Success => _success;

    public IEnumerable<AppError>? Errors => _errors;

    public string? Message => _message;

}
public class AppResponce<TData> : AppResponce
{
    private readonly TData? _data;

    public AppResponce(TData data) : base()
    {
        _data = data;
    }

    public AppResponce(AppError error) : base(error)
    {

    }

    public AppResponce(string message) : base(message)
    {

    }

    public AppResponce(string message, bool success) : base(message,success)
    {

    }

    public AppResponce(IEnumerable<AppError> errors) : base(errors)
    {

    }

#pragma warning disable
    public TData Data => _data;


}