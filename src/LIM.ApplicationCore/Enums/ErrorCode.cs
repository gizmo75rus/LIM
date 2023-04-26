using System.ComponentModel;
using System.Text.Json.Serialization;

namespace LIM.ApplicationCore.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ErrorCode
{
    [Description("Внутреняя ошибка")]
    InternalError = 100,
    
    [Description("Не найдено")]
    NotFound = 200,
    
    [Description("Запись существует")]
    RecordExist = 201,
    
    [Description("Действие запрещено")]
    ActionForbidden = 300,
}