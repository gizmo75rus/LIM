using FluentValidation;
using LIM.SharedKernel.Extensions;

namespace LIM.WebApp.Extensions;

public static class FluentValidatorExtensions
{
    public static IRuleBuilder<T, string> IsRequired<T>(this IRuleBuilder<T, string> ruleBuilder, string errorMessage)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage(errorMessage)
            .NotNull()
            .WithMessage(errorMessage);
    }

    public static IRuleBuilder<T, int> IsRequired<T>(this IRuleBuilder<T, int> ruleBuilder, string errorMessage)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage(errorMessage)
            .NotNull()
            .WithMessage(errorMessage);
    }

    public static IRuleBuilder<T,string> IsPhone<T>(this IRuleBuilder<T,string> ruleBuilder,string errorMessage){
        return ruleBuilder
            .Must(x=> string.IsNullOrEmpty(x) || x.IsPhoneString())
            .WithMessage(errorMessage);
    }
}