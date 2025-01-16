using LIM.ApplicationCore.BaseObjects;
using LIM.SharedKernel.Interfaces;

namespace LIM.ApplicationCore.Models;

/// <summary>
/// Производитель
/// </summary>
public class Manufacturer : BaseEntity<int>
{
    public string? Name { get; set; }
    public HashSet<Instrument> Instruments { get; set; } = new HashSet<Instrument>();

    public Manufacturer()
    {
        
    }

    public Manufacturer(string name)
    {
        Name = name;
    }
}