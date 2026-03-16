namespace Shared.Core.Entities;

public enum Roles
{
    Administrador,
    Vendedor
}

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    // ESTAS PROPIEDADES SON LAS QUE FALTABAN:
    public string Email { get; set; } = string.Empty;
    public string ContrasenaHash { get; set; } = string.Empty;
    public Roles Rol { get; set; } = Roles.Administrador;
    public bool Estado { get; set; } = true;

    // Propiedades para integraciones y auditoría    
    public string? Telefono { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}