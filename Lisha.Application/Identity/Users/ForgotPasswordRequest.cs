﻿using Lisha.Application.Common.Validation;

namespace Lisha.Application.Identity.Users
{
    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = default!;
    }

    public class ForgotPasswordRequestValidator : CustomValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordRequestValidator() =>
            RuleFor(p => p.Email).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .EmailAddress()
                    .WithMessage("Invalid Email Address.");
    }
}
