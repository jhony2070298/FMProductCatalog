using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Application.Common;
using ProductCatalog.Application.Products;
using ProductCatalog.Application.Products.Dtos;

namespace ProductCatalog.Api.Controllers
{
    [ApiController]
    [Route("api/products")]
    [Produces("application/json")]
    public sealed class ProductsController(IProductService productService) : ControllerBase
    {
        /// <summary>Lista los productos de forma paginada.</summary>
        /// <param name="page">Número de página (desde 1).</param>
        /// <param name="pageSize">Tamaño de página (1 a 100).</param>
        /// <param name="ct">Token de cancelación.</param>
        [HttpGet]
        [ProducesResponseType<PagedResult<ProductResponse>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedResult<ProductResponse>>> GetAll(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
            Ok(await productService.GetPagedAsync(new GetProductsQuery(page, pageSize), ct));

        /// <summary>Obtiene un producto por su id.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductResponse>> GetById(Guid id, CancellationToken ct) =>
            Ok(await productService.GetByIdAsync(id, ct));

        /// <summary>Crea un producto con su stock inicial.</summary>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType<ProductResponse>(StatusCodes.Status201Created)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request, CancellationToken ct)
        {
            var product = await productService.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        /// <summary>Actualiza nombre, descripción y precio. El stock se modifica solo por su endpoint.</summary>
        [HttpPut("{id:guid}")]
        [Consumes("application/json")]
        [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ProductResponse>> Update(Guid id, UpdateProductRequest request, CancellationToken ct) =>
            Ok(await productService.UpdateAsync(id, request, ct));

        /// <summary>Suma o resta unidades al stock actual.</summary>
        /// <remarks>
        /// Envíe una cantidad positiva para sumar y negativa para restar. Ejemplo: <c>{ "quantity": -3 }</c>.
        /// El stock resultante nunca puede quedar negativo (422). Las peticiones concurrentes
        /// se resuelven con concurrencia optimista y reintentos (409 si se agotan).
        /// </remarks>
        [HttpPatch("{id:guid}/stock")]
        [Consumes("application/json")]
        [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ProductResponse>> AdjustStock(Guid id, AdjustStockRequest request, CancellationToken ct) =>
            Ok(await productService.AdjustStockAsync(id, request, ct));

        /// <summary>Elimina un producto.</summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await productService.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
