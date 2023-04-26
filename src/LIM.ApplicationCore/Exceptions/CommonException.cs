using LIM.ApplicationCore.Enums;

namespace LIM.ApplicationCore.Exceptions;

public class CommonException : Exception
{
    public ErrorCode ErrorCode { get; set; } = ErrorCode.InternalError;

    public CommonException(string message) : base(message)
    {
        
    }

    public CommonException(ErrorCode errorCode, string message) : base(message)
    {
        this.ErrorCode = errorCode;
    }


    public static CommonException NotFound => new CommonException(ErrorCode.NotFound, "Запись не найдена");
    public static CommonException RecordExist => new CommonException(ErrorCode.RecordExist, "Запись существует");
}