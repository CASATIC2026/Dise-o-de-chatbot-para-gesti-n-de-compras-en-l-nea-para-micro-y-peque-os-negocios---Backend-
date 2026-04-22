using Shared.Core.Entities;
using Services.ChatBot.DTOs;
namespace Services.ChatBot.Interfaces;

/// <summary>
/// Defines the contract for bot persistence operations, including managing conversations,
/// client data, shopping carts, and order history.
/// </summary>
public interface IBotPersistencia
{
    /// <summary>
    /// Retrieves the active conversation for a given client Telegram ID.
    /// </summary>
    /// <param name="clienteId">The Telegram ID of the client.</param>
    /// <returns>The active <see cref="Conversacion"/> if found, otherwise null.</returns>
    Task<Conversacion?> ObtenerConversacionActiva(long clienteId);

    /// <summary>
    /// Updates an existing conversation or creates a new one, setting its active status.
    /// </summary>
    /// <param name="clienteId">The Telegram ID of the client.</param>
    /// <param name="messageId">The message ID to associate with the conversation, often the ID of the bot's last message.</param>
    /// <param name="activa">A boolean indicating whether the conversation should be marked as active.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ActualizarConversacion(long clienteId, int messageId, bool activa);

    /// <summary>
    /// Registers a new message within a specific conversation.
    /// </summary>
    /// <param name="conversacionId">The unique identifier of the conversation.</param>
    /// <param name="contenido">The text content of the message.</param>
    /// <param name="remitente">The type of sender (e.g., Client, System).</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task RegistrarMensaje(int conversacionId, string contenido, TipoRemitente remitente);

    /// <summary>
    /// Registers a new client if they do not already exist in the system.
    /// </summary>
    /// <param name="TelegramId">The unique Telegram ID of the client.</param>
    /// <param name="nombre">The name of the client.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task RegistrarCliente(long TelegramId, string nombre);

    /// <summary>
    /// Adds a specified quantity of a product to the client's active shopping cart.
    /// Manages stock reservation and updates the order total.
    /// </summary>
    /// <param name="TelegramId">The Telegram ID of the client.</param>
    /// <param name="productoId">The unique identifier of the product to add.</param>
    /// <param name="cantidad">The quantity of the product to add.</param>
    /// <returns>A tuple indicating success and a descriptive message.</returns>
    Task<(bool Success, string msg)> AgregarProducto(long TelegramId, int productoId, int cantidad);

    /// <summary>
    /// Retrieves the active, pending order for a given client.
    /// </summary>
    /// <param name="TelegramId">The Telegram ID of the client.</param>
    /// <returns>The active <see cref="Pedido"/> if found, otherwise null.</returns>
    Task<Pedido?> ObtenerPedidoActivo(long TelegramId);

    /// <summary>
    /// Retrieves client information by their Telegram ID.
    /// </summary>
    /// <param name="TelegramId">The Telegram ID of the client.</param>
    /// <returns>The <see cref="Cliente"/> object if found, otherwise null.</returns>
    Task<Cliente?> ObtenerCliente(long TelegramId);

    /// <summary>
    /// Clears all items from the client's active shopping cart and cancels the associated order.
    /// </summary>
    /// <param name="TelegramId">The Telegram ID of the client.</param>
    /// <returns>True if the cart was successfully emptied, otherwise false.</returns>
    Task<bool> VaciarCarrito(long TelegramId);

    /// <summary>
    /// Removes a specific item from the client's active shopping cart.
    /// </summary>
    /// <param name="TelegramId">The Telegram ID of the client.</param>
    /// <param name="productoId">The unique identifier of the product to remove.</param>
    /// <returns>A tuple indicating success and a descriptive message.</returns>
    Task<(bool Succes, string msg)> EliminarItem(long TelegramId, int productoId);

    /// <summary>
    /// Updates the quantity of a specific product within the client's active shopping cart.
    /// </summary>
    /// <param name="TelegramId">The Telegram ID of the client.</param>
    /// <param name="productoId">The unique identifier of the product.</param>
    /// <param name="cantidad">The new quantity for the product.</param>
    /// <returns>A tuple indicating success and a descriptive message.</returns>
    Task<(bool Succes, string msg)> ActualizarCantidadCarrito(long TelegramId, int productoId, int cantidad);

    /// <summary>
    /// Updates the client's profile information based on the provided DTO.
    /// </summary>
    /// <param name="dtoC">A <see cref="ClienteDTO"/> containing the updated client information.</param>
    /// <returns>True if the client information was successfully updated, otherwise false.</returns>
    Task<bool> ActualizarCliente(ClienteDTO dtoC);

    /// <summary>
    /// Retrieves a paginated list of orders for a specific user.
    /// </summary>
    /// <param name="TelegramId">The Telegram ID of the client.</param>
    /// <param name="tamaño">The number of orders per page.</param>
    /// <param name="pagina">The page number to retrieve (0-indexed).</param>
    /// <returns>A tuple containing a list of <see cref="Pedido"/> objects and the total count of orders.</returns>
    Task<(List<Pedido>, int count)> ObtenerPedidosUsuario(long TelegramId, int tamaño, int pagina);

    /// <summary>
    /// Updates the details of the client's active order.
    /// </summary>
    /// <param name="TelegramId">The Telegram ID of the client.</param>
    /// <param name="pdd">A <see cref="PedidoDTO"/> containing the updated order details.</param>
    /// <returns>A tuple indicating success and a descriptive message.</returns>
    Task<(bool Succes, string msg)> ActualizarPedido(long TelegramId, PedidoDTO pdd);

    /// <summary>
    /// Retrieves the most recent message from a specific conversation.
    /// </summary>
    /// <param name="conversacionId">The unique identifier of the conversation.</param>
    /// <returns>The latest <see cref="Mensaje"/> from the conversation, or null if no messages exist.</returns>
    Task<Mensaje?> ObtenerUltimoMensaje(int conversacionId);
}
