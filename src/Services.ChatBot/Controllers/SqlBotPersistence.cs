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
        var cliente = await context.Clientes.FirstOrDefaultAsync(c => c.TelegramId == clienteId);
        if (cliente == null ) return null;
        return await context.Conversaciones.FirstOrDefaultAsync(c => c.ClienteId == cliente.Id && c.Activa == true);
    }

    public async Task ActualizarConversacion(long clienteId, int messageId, bool activa)
    {
        var cliente = await context.Clientes.FirstOrDefaultAsync(c => c.TelegramId == clienteId);
        var elder = await context.Conversaciones.Where(c => c.ClienteId == cliente!.Id && c.Activa).ToListAsync();

        foreach (var v in elder)
        {
            v.Activa = false;
            v.ActualizadoEn = DateTime.UtcNow;
        }
        
        //var conv = await context.Conversaciones.FirstOrDefaultAsync(c => c.ClienteId == cliente!.Id);
        var conv = await ObtenerConversacionActiva(cliente!.Id);
        Console.WriteLine("conversacion", conv);
        if (conv == null)
        {
            conv = new Conversacion { ClienteId = (int)cliente!.Id, CreadoEn = DateTime.UtcNow };
            context.Conversaciones.Add(conv);
        } else { return; }
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
        context.Conversaciones.FirstOrDefault(c=> c.Id == conversacionId).ActualizadoEn = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task RegistrarCliente(long TelegramId, string nombre)
    {
        var cliente = await context.Clientes.FirstOrDefaultAsync(c => c.TelegramId == TelegramId);
        //Console.WriteLine("s"+cliente.TelegramId);
        if (cliente == null)
        {
            Console.WriteLine("s"+TelegramId +" "+nombre );
            context.Clientes.Add(
                new Cliente
                {
                    TelegramId = TelegramId,
                    Nombre = nombre ?? "Usuario Telegram",
                    CreadoEn = DateTime.UtcNow,
                    ActualizadoEn = DateTime.UtcNow
                }
            );
            await context.SaveChangesAsync();
        }
    }

}