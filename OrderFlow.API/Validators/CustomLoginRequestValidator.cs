using FluentValidation;
using OrderFlow.API.Endpoints.DTOs;

namespace OrderFlow.API.Validators;

public class CustomLoginRequestValidator : AbstractValidator<CustomLoginRequest>
{
    public CustomLoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("O nome de usuário é obrigatório.")
            .Length(3, 50).WithMessage("O usuário deve ter entre 3 e 50 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A senha é obrigatória.")
            .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.");
    }
}