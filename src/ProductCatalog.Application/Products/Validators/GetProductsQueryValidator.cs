using FluentValidation;
using ProductCatalog.Application.Products.Dtos;

namespace ProductCatalog.Application.Products.Validators
{
    public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
    {
        public const int MaxPageSize = 100;

        public GetProductsQueryValidator()
        {
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).InclusiveBetween(1, MaxPageSize);
        }
    }
}
