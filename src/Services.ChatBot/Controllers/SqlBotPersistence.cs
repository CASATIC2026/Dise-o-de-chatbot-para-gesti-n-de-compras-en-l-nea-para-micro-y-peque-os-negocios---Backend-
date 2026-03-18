using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Services.ChatBot.Interfaces;
using Shared.Core.Data;
using Shared.Core.Entities;

namespace Webhook.Controllers.Controllers;

public class SqlBotPersistence(ApplicationDbContext context) : IBotPersistencia
{
    public async Task<Conversacion?> ObtenerConversacionActiva(long clienteId)
    {
        return await context.Conversaciones.FirstOrDefaultAsync(c => c.ClienteId == clienteId && c.Activa == true);
    }

    public async Task ActualizarConversacion(int clienteId, int messageId, bool activa)
    {
        var conv = await context.Conversaciones.FirstOrDefaultAsync(c => c.ClienteId == clienteId);
        if (conv != null)
        {
            conv = new Conversacion { ClienteId = (int)clienteId, CreadoEn = DateTime.UtcNow };
            context.Conversaciones.Add(conv);
        }
        else { return; }
        conv.Asunto = messageId.ToString();
        conv.Activa = true;
        conv.ActualizadoEn = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task RegistrarMensaje(int conversacionId, string contenido, TipoRemitente remitente)
    {
        context.Mensajes.Add(new Mensaje
        {
            ConversacionId = conversacionId,
            Contenido = contenido,
            Remitente = remitente,
            FechaEnvio = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }

}