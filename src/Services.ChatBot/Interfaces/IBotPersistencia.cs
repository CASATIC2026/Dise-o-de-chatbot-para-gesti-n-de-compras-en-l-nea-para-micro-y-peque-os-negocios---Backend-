using Shared.Core.Entities;

namespace Services.ChatBot.Interfaces;

public interface IBotPersistencia
{
    Task<Conversacion?> ObtenerConversacionActiva(long clienteId);
    Task ActualizarConversacion(long clienteId, int messageId, bool activa);
    Task RegistrarMensaje(int conversacionId, string contenido, TipoRemitente remitente);
    Task RegistrarCliente(long TelegramId, string nombre);
}

