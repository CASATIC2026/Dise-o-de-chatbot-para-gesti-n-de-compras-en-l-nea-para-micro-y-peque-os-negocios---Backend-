using Shared.Core.Entities;

namespace Services.ChatBot.Interfaces;

public interface IBotPersistencia
{
    Task<Conversacion?> ObtenerConversacionActiva(long clienteId);
    Task ActualizarConversacion(long clienteId, int messageId, bool activa);
    Task RegistrarMensaje(int conversacionId, string contenido, TipoRemitente remitente);
    Task RegistrarCliente(long TelegramId, string nombre);
    Task<(bool Success, string msg)> AgregarProducto(long TelegramId, int productoId, int cantidad);
    Task<Pedido?> ObtenerPedidoActivo(long TelegramId);
    Task<bool> VaciarCarrito(long TelegramId);
    Task<(bool Succes, string msg)> EliminarItem(long TelegramId, int productoId);
    Task<(bool Succes, string msg)> ActualizarCantidadCarrito(long TelegramId, int productoId, int cantidad);
}

