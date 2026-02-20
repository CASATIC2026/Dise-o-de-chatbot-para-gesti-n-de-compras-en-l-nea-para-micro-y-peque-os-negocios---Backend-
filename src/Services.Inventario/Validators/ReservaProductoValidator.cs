using FluentValidation;

namespace Services.Inventario.Validators;

public class ReservaProductoRequest
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public string? ReservaId { get; set; }
}

public class ReservaProductoValidator : AbstractValidator<ReservaProductoRequest>
{
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
