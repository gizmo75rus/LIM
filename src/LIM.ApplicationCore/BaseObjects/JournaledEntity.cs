using LIM.SharedKernel.Interfaces;

namespace LIM.ApplicationCore.BaseObjects;

public class JournaledEntity<Tid> : BaseEntity<Tid>,IJournaledEntity
{
    /// <inheritdoc/>
    public DateTime CreatedAt { get; set; }
    
    /// <inheritdoc/>
    public DateTime? UpdatedAt { get; set; }
    
    /// <inheritdoc/>
    public string? CreatedBy { get; set; }
    
    /// <inheritdoc/>
    public string? UpdatedBy { get; set; }

}