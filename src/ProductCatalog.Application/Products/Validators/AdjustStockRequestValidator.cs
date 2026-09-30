using FluentValidation;
using ProductCatalog.Application.Products.Dtos;

namespace ProductCatalog.Application.Products.Validators
{
    public sealed class AdjustStockRequestValidator : AbstractValidator<AdjustStockRequest>
    {
        public AdjustStockRequestValidator()
        {
            RuleFor(x => x.Quantity)
                .NotEqual(0).WithMessage("La cantidad debe ser distinta de cero (positiva para sumar, negativa para restar).");
        }
    }
}
