using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Application.Common.Exceptions;
using ProductCatalog.Domain.Exceptions;

namespace ProductCatalog.Api.Middleware
{
    internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            ProblemDetails problem = exception switch
            {
                ValidationException ex => new ValidationProblemDetails(
                    ex.Errors
                      .GroupBy(e => e.PropertyName)
                      .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Error de validación"
                },

                DomainValidationException ex => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Regla de negocio no cumplida",
                    Detail = ex.Message
                },

                NotFoundException ex => new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Recurso no encontrado",
                    Detail = ex.Message
                },

                InsufficientStockException ex => new ProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Title = "Stock insuficiente",
                    Detail = ex.Message,
                    Extensions =
                {
                    ["currentStock"] = ex.CurrentStock,
                    ["requestedAdjustment"] = ex.RequestedAdjustment
                }
                },

                ConcurrencyConflictException ex => new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Conflicto de concurrencia",
                    Detail = ex.Message
                },

                _ => new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error inesperado. Use el traceId para reportarlo."
                }
            };

            if (problem.Status >= 500)
                logger.LogError(exception, "Error no controlado procesando {Method} {Path}",
                    httpContext.Request.Method, httpContext.Request.Path);
            else
                logger.LogInformation("Petición rechazada ({Status}): {Message}", problem.Status, exception.Message);

            httpContext.Response.StatusCode = problem.Status!.Value;

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem,
                Exception = exception
            });
        }
    }
}
