using System.Data;
using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.IdentityModel.Tokens;

namespace Services.ChatBot.Utils;

public class DeserialiceJson
{
    private readonly string _filePath;

    public DeserialiceJson()
    {
        _filePath = Path.Combine(AppContext.BaseDirectory, "Files", "Geo.json");
    }
    public Dictionary<String, List<String>> ObtenerDatos()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                throw new FileNotFoundException($"El archivo {_filePath} no existe.");        
            }

            string jsonString = File.ReadAllText(_filePath);
            var estruturaC = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<string>>>>(jsonString);

            if(estruturaC != null && estruturaC.TryGetValue("Departamento", out var departamentos))
            {
                return departamentos;
            }
            return new Dictionary<string, List<string>>();

        }
        catch
        {
            Console.WriteLine($"Error al leer el archivo {_filePath}.");
            return new Dictionary<string, List<string>>();
        }
    }
}