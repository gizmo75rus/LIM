namespace LIM.WebApp.Models;

public class AppResponce
{
    private readonly bool _success;
    private readonly IEnumerable<AppError>? _errors;
    private string? _message;

    public static AppResponce Ok()
    {
        return new AppResponce
        {
            Success = true,
        };
    }

    public static AppResponce Ok(string messsage)
    {
        return new AppResponce
        {
            Success = true,
            _message = messsage
        };
    }

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


    public bool Success { get; set; }


    public IEnumerable<AppError>? Errors => _errors;

    public string? Message => _message;

}
public class AppResponce<TData> : AppResponce where TData : class
{
    private readonly TData? _data;

    private AppResponce(TData data) : base()
    {
        _data = data;
        Success = true;
    }
    
    public static AppResponce<TData> Ok(TData data) => new AppResponce<TData>(data);

#pragma warning disable
    public TData Data => _data;


}