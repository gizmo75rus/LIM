using FluentValidation;
using FluentValidation.Results;
using LIM.ApplicationCore.Enums;
using LIM.WebApp.Extensions;

namespace LIM.WebApp.Models;

public class DeviceRequestModel
{
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;

    public ProtocolType ProtocolType { get; set; } = ProtocolType.ASTM;
}

public class DeviceViewModelValidator : AbstractValidator<DeviceRequestModel>
{
    public DeviceViewModelValidator()
    {
        RuleFor(x => x.Manufacturer)
            .IsRequired("Не указан производитель");
        RuleFor(x => x.Model)
            .IsRequired("Ну указан модель");

    }
    
}