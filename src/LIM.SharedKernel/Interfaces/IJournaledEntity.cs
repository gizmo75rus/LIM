namespace LIM.SharedKernel.Interfaces;

public interface IJournaledEntity
{
    /// <summary>
    /// когда создано
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// кем создано
    /// </summary>
    string? CreatedBy { get; set; }
    
    /// <summary>
    /// когда обновлено
    /// </summary>
    DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// кем обновлено
    /// </summary>
    string? UpdatedBy { get; set; }
}