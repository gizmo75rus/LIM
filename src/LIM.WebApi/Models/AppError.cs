using LIM.ApplicationCore.Enums;

namespace LIM.WebApp.Models;

public class AppError
{
    public ErrorCode Code { get; private set; }

    public string Message { get; private set; }

    public AppError(string message)
    {
        Message = message;
        Code = ErrorCode.Error;
    }

    public AppError(ErrorCode errorCode)
    {
        Code = errorCode;
        Message = string.Empty;
    }

    public AppError(ErrorCode errorCode, string message)
    {
        Code = errorCode;
        Message = message;
    }

}