namespace Shared.Core.Entities;

/// <summary>
/// Defines the available authorization roles for users within the system.
/// </summary>
public enum Roles
{
    /// <summary>User with full system access and administrative privileges.</summary>
    Administrador = 1,
    /// <summary>User with limited access primarily for sales and order management.</summary>
    Vendedor = 2
}

/// <summary>
/// Represents a system user (administrator or staff) capable of logging in and managing the inventory and orders.
/// </summary>
public class Usuario
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the full name of the user.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address used for login and notifications.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hashed version of the user's password.
    /// </summary>
    public string ContrasenaHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the assigned role, determining the user's permission level.
    /// </summary>
    public Roles Rol { get; set; } = Roles.Vendedor;

    /// <summary>
    /// Gets or sets a value indicating whether the user account is active.
    /// </summary>
    public bool Estado { get; set; } = true;

    /// <summary>
    /// Gets or sets the contact phone number of the user.
    /// </summary>
    public string? Telefono { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user record was created.
    /// </summary>
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the user record was last updated.
    /// </summary>
    public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the collection of orders managed or associated with this user.
    /// </summary>
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
