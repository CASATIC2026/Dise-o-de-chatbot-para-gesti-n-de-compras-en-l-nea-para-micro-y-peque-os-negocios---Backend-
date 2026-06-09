using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shared.Core.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Services.Inventario.Controllers;

/// <summary>
/// Controller responsible for handling authentication and authorization requests.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="context">The database context used to access user information.</param>
    /// <param name="config">The configuration used to retrieve security settings like JWT secrets.</param>
    public AuthController(ApplicationDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    /// <summary>
    /// Authenticates a user based on email and password and returns a JWT token.
    /// </summary>
    /// <param name="request">The login request details.</param>
    /// <returns>An <see cref="IActionResult"/> containing the JWT token and user info if successful, or Unauthorized if not.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == request.Email);
        
        /*
        Console.WriteLine($"Email: {request.Email}, Password: {request.Password}");
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
        var jwtSecret = _config["JWT_SECRET"];
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

/// <summary>
/// Represents a request to log in to the system.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// The email address of the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The plain-text password of the user.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}