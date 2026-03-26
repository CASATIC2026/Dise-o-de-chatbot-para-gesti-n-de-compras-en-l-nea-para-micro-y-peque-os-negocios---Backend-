using Telegram.Bot.Types.ReplyMarkups;

namespace Services.ChatBot.Interfaces;

public interface IUtilsUI
{
    // Métodos para generar teclados personalizados
    //Quitar teclado inline
    Task InvalidarMenu(long chatId, int messageId, string textoAviso, string action);
    // Método para eliminar un mensaje específico
    Task EliminarMensaje(long chatId, int messageId);
    
    InlineKeyboardMarkup? LimpiarKeyboard();// Método para limpiar el teclado inline
}

