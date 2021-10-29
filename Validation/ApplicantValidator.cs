using FluentValidation;
using Models;
using System;

namespace Validation
{
    public class ApplicantValidator : AbstractValidator<Applicant>
    {
        public ApplicantValidator()
        {
            RuleFor(a => a.Email).NotEmpty().EmailAddress();
            RuleFor(a => a.FirstName).NotEmpty().MaximumLength(50);
            RuleFor(a => a.LastName).NotEmpty().MaximumLength(50);
            RuleFor(a => a.Loans).GreaterThanOrEqualTo(0);
            RuleFor(a => a.Income).GreaterThanOrEqualTo(0);
        }
    }
}
