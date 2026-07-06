using Elattba.Core.DTOs;
using FluentValidation;

namespace Elattba.Application.Validation;

public sealed class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        Include(new UserRules<CreateUserDto>(
            user => user.Email,
            user => user.Phone,
            user => user.Role,
            user => user.GovernorateId,
            user => user.City,
            user => user.ShippingAddress));
        RuleFor(user => user.Password).NotEmpty().MinimumLength(8);
    }
}

public sealed class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        Include(new UserRules<UpdateUserDto>(
            user => user.Email,
            user => user.Phone,
            user => user.Role,
            user => user.GovernorateId,
            user => user.City,
            user => user.ShippingAddress));
    }
}

internal sealed class UserRules<T> : AbstractValidator<T>
{
    public UserRules(
        Func<T, string> email,
        Func<T, string?> phone,
        Func<T, ElAtaba.Domain.Enums.UserRole> role,
        Func<T, int> governorateId,
        Func<T, string> city,
        Func<T, string> shippingAddress)
    {
        RuleFor(user => email(user)).NotEmpty().EmailAddress().WithName("Email");
        RuleFor(user => phone(user)).MaximumLength(30).WithName("Phone");
        RuleFor(user => role(user)).IsInEnum().WithName("Role");
        RuleFor(user => governorateId(user)).GreaterThan(0).WithName("GovernorateId");
        RuleFor(user => city(user)).NotEmpty().WithName("City");
        RuleFor(user => shippingAddress(user)).NotEmpty().WithName("ShippingAddress");
    }
}
