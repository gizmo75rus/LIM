using LIM.SharedKernel.Interfaces;

namespace LIM.ApplicationCore.BaseObjects;

public class BaseEntity<Tid> : IEntity
{
#pragma warning disable CS8618
    public Tid Id { get; set; }
}