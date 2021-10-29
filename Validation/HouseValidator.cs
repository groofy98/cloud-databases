using FluentValidation;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validation
{
    public class HouseValidator : AbstractValidator<House>
    {
        public HouseValidator()
        {
            RuleFor(h => h.HouseNumber).NotEmpty();
            RuleFor(h => h.Street).NotEmpty();
            RuleFor(h => h.Price).GreaterThan(0);
            RuleFor(h => h.Town).NotEmpty();
            RuleFor(h => h.PostalCode).NotEmpty().Matches("^[1-9][0-9]{3} ?(?!sa|sd|ss|SA|SD|SS)[A-Za-z]{2}$");
        }
    }
}
