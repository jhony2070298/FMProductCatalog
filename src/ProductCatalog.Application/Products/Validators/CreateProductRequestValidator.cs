using FluentValidation;
using ProductCatalog.Application.Products.Dtos;
using ProductCatalog.Domain.Products;

namespace ProductCatalog.Application.Products.Validators
{
    public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MaximumLength(Product.NameMaxLength);

            RuleFor(x => x.Description)
                .MaximumLength(Product.DescriptionMaxLength);

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("El precio debe ser mayor que cero.")
                .PrecisionScale(18, 2, ignoreTrailingZeros: true);

            RuleFor(x => x.InitialStock)
                .GreaterThanOrEqualTo(0).WithMessage("El stock inicial no puede ser negativo.");
        }
    }
}
