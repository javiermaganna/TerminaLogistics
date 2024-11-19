using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoListApp.Data;
using TodoListApp.Models;
using TodoListApp.Models.DTOs;

namespace TodoListApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Registro de un nuevo usuario
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            // Verificar si el usuario ya existe
            if (await _context.Usuarios.AnyAsync(u => u.Username == registerDto.Username))
            {
                return BadRequest(new { message = "El nombre de usuario ya está en uso." });
            }

            // Crear un nuevo usuario con la contraseña hasheada
            var user = new Usuario
            {
                Username = registerDto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password) // Hash de la contraseña
            };

            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario registrado con éxito" });
        }

        // Login de un usuario
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // Buscar usuario por nombre de usuario
            var user = await _context.Usuarios.SingleOrDefaultAsync(u => u.Username == loginDto.Username);

            // Validar si el usuario existe y si la contraseña es correcta
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return BadRequest(new { message = "Correo electrónico o contraseña incorrectos." });
            }

            // Generar token JWT
            var tokenString = GenerateToken(user);

            // Depuración del token generado
            Console.WriteLine($"Token generado: {tokenString}");

            // Respuesta detallada con información adicional
            return Ok(new
            {
                success = true,
                token = tokenString,  // Token generado
                email = user.Username,  // Devuelve el nombre de usuario en la respuesta
                userId = user.Id,  // Devuelve el ID de usuario en la respuesta
                redirectUrl = Url.Action("Index", "Home")  // URL de redirección (ajustar según lo que necesites)
            });
        }

        // Método para generar el token JWT
        private string GenerateToken(Usuario user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // ID del usuario como "NameIdentifier"
                new Claim(ClaimTypes.Name, user.Username) // Nombre de usuario como "Name"
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1), // Configura la expiración del token
                signingCredentials: creds
            );

            // Devuelve el token JWT como un string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
