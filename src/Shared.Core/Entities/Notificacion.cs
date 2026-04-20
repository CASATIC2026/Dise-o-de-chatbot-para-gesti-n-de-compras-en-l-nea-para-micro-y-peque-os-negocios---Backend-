using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Shared.Core.Entities
{
    public class Notificacion
    {
        public int Id {get; set;}
        public string Titulo {get; set;}
        public string Mensaje {get; set;}
        public string Tipo {get; set;} = "Info"; // info, succes, warning, error
        public DateTime Fecha {get; set;} = DateTime.UtcNow;

    }
}