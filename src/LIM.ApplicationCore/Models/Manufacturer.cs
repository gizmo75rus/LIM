using LIM.ApplicationCore.BaseObjects;
using LIM.SharedKernel.Interfaces;

namespace LIM.ApplicationCore.Models;

/// <summary>
/// Производитель
/// </summary>
public class Manufacturer : BaseEntity<int>
{
    public string? Name { get; set; }
    public HashSet<Device> Devices { get; set; } = new HashSet<Device>();

    public Manufacturer()
    {
        
    }

    public Manufacturer(string name)
    {
        Name = name;
    }
}