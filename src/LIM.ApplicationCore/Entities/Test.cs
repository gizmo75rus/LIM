using LIM.ApplicationCore.BaseObjects;
using LIM.ApplicationCore.Enums;

namespace LIM.ApplicationCore.Entities;

/// <summary>
/// тест выполняемый инструментом
/// </summary>
public class Test : JournaledEntity<int>
{
    public Guid? ConsumerDeviceId { get; set; }

    /// <summary>
    /// Тип биоматериала
    /// </summary>
    public Biomaterial? Biomaterial { get; set; }
    
    /// <summary>
    /// Номер теста в приборе
    /// </summary>
    public int? Number { get; set; }
    
    /// <summary>
    /// Код теста в приборе
    /// </summary>
    public string? Code { get; set; }
    
    /// <summary>
    /// Идентификатор теста в ЛИС
    /// </summary>
    public string? LisTestId { get; set; }
    
    /// <summary>
    /// Идентификатор теста в МИС
    /// </summary>
    public string? MisTestId { get; set; }
    
    /// <summary>
    /// Наименование теста
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// применяется на устройстве
    /// </summary>
    public ConsumerDevice? ConsumerDevice { get; set; }
}