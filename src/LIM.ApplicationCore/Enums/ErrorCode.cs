using System.ComponentModel;
using System.Text.Json.Serialization;

namespace LIM.ApplicationCore.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ErrorCode
{
    [Description("Внутреняя ошибка")]
    InternalError = 100,
    
    [Description("Необработанное исключение")]
    UnhandeledExeption = 101,
    
    [Description("Не найдено")]
    NotFound = 200,
    
    [Description("Запись существует")]
    RecordExist = 201,
    
    [Description("Ошибка")]
    Error = 202,
    
    [Description("")]
    ValidationError = 203,
    
    [Description("Доступ запрещен")]
    AccessDenied = 300,
    
    [Description("Действие запрещено")]
    ActionForbidden = 301,
  
}