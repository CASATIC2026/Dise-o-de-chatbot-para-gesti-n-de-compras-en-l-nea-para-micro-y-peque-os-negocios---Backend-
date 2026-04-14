using Telegram.Bot.Types;
namespace Services.ChatBot.Utils;
/// <summary>
/// Clase auxiliar para simular un objeto Message de la API de Telegram.
/// Se utiliza para permitir la asignación manual del MessageId, ya que en la 
/// clase base 'Message' dicha propiedad es de solo lectura (read-only).
/// </summary>
public class MockMessage : Message
{
    /// <summary>
    /// Sobrescribe la propiedad MessageId original con el modificador 'new' 
    /// para permitir la escritura (set) de IDs provenientes de nuestra base de datos.
    /// </summary>
    public new int MessageId { get; set; }
}