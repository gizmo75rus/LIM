using LIM.SharedKernel.Interfaces;

namespace LIM.ApplicationCore.BaseObjects;

public class BaseEntity<Tid> : IEntity
{
    public Tid Id { get; set; }
}