using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shared.Core.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Services.Inventario.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(ApplicationDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == request.Email);
        
        /*Console.WriteLine($"Email: {request.Email}, Password: {request.Password}");
        Console.WriteLine("Hash en DB: {hash}" + usuario.ContrasenaHash + " " + usuario.ContrasenaHash.Length);
        Console.WriteLine(BCrypt.Net.BCrypt.HashPassword(request.Password));
        if (usuario == null)
        {
            Console.WriteLine("Usuario no encontrado en la DB");
            return Unauthorized();
        }
        bool esValido = BCrypt.Net.BCrypt.Verify(request.Password.Trim(), usuario.ContrasenaHash.Trim());
        if (!esValido)
        {
            Console.WriteLine("Contraseña incorrecta");
            return Unauthorized();
        }*/

        if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.ContrasenaHash))
        {
            return Unauthorized(new { message = "Email o contraseña incorrectos" });
        }

        // 2. Preparamos la llave secreta desde el .env
        var jwtSecret = _config["JWT_SECRET"] ?? "f9a2b8c7e6d5a4b3c2d1e0f9a8b7c6d5e4f3a2b1c0d9e8f7a6b5c4d3e2f1a0b";
        var key = Encoding.ASCII.GetBytes(jwtSecret);

        // 3. Creamos el contenido del token (Claims)
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol.ToString() ?? "Administrador")
            }),
            Expires = DateTime.UtcNow.AddHours(8), // Basado en tu JWT_EXPIRATION=8h
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        // 4. Devolvemos el token y datos básicos del usuario
        return Ok(new
        {
            Token = tokenHandler.WriteToken(token),
            User = new
            {
                usuario.Nombre,
                usuario.Email,
                usuario.Rol
            }
        });
    }
}

// Clase para recibir los datos del post
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}