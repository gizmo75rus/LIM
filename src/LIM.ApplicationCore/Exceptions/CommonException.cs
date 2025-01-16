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


    public static CommonException NotFound => 
        new CommonException(ErrorCode.NotFound, "Запись не найдена");
    
    public static CommonException RecordExist => 
        new CommonException(ErrorCode.RecordExist, "Запись существует");

    public static CommonException FailedToSaveObject =>
        new CommonException(ErrorCode.FailedToSaveObject, "Не удалось сохранить объект");

    public static CommonException ArgumentOutException =>
        new CommonException(ErrorCode.ValidationError, "Аргумент за пределами допустимых значений");

    public static CommonException ReferencesToObjectNotFree =>
        new CommonException(ErrorCode.ReferencesToObjectNotFree, "Имеются связи на указанный объект ");
}