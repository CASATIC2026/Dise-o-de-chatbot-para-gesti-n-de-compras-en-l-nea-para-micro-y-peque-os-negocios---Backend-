using FluentValidation;

namespace Services.Inventario.Validators;

/// <summary>
/// Represents a request to temporarily reserve stock for a specific product.
/// </summary>
public class ReservaProductoRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the product to be reserved.
    /// </summary>
    public int ProductoId { get; set; }

    /// <summary>
    /// Gets or sets the quantity of stock to reserve.
    /// </summary>
    public int Cantidad { get; set; }

    /// <summary>
    /// Gets or sets an optional unique identifier for the reservation. 
    /// If not provided, a new Guid is typically generated server-side.
    /// </summary>
    public string? ReservaId { get; set; }
}

/// <summary>
/// Validator for <see cref="ReservaProductoRequest"/> that ensures business rules for stock reservation are met.
/// </summary>
public class ReservaProductoValidator : AbstractValidator<ReservaProductoRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReservaProductoValidator"/> class and defines validation rules.
    /// </summary>
    public ReservaProductoValidator()
    {
        RuleFor(x => x.ProductoId)
            .GreaterThan(0)
            .WithMessage("El ID del producto debe ser mayor a 0");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0)
            .WithMessage("La cantidad debe ser mayor a 0")
            .LessThanOrEqualTo(100)
            .WithMessage("La cantidad máxima por pedido es 100");

        RuleFor(x => x.ReservaId)
            .NotEmpty()
            .When(x => x.ReservaId != null)
            .WithMessage("El ID de reserva no puede estar vacío");
    }
}
