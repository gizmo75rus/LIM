using System.ComponentModel;
using System.Text.Json.Serialization;

namespace LIM.ApplicationCore.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Biomaterial
{
    [Description("Кровь")]
    Blood,
    
    [Description("Моча")]
    Urine,
    
    [Description("Кал")]
    Excrement,
    
    [Description("Мазок из горла")]
    ThroatSwab,
    
    [Description("мазок из уретры")]   
    SwabFromTheUrethra,
    
    [Description("мазок из влагалища")]   
    SwabFromVaginal,
    
    [Description("Образец биопсии")]
    Biopsy
}