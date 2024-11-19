using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading;
using TodoListApp.Data;
using TodoListApp.Models;
using TodoListApp.Models.DTOs;

namespace TodoListApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Proteger el controlador
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Método para obtener el ID del usuario autenticado desde el token JWT
        private int? GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userIdClaim != null ? int.Parse(userIdClaim) : (int?)null;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TareaDTO>>> GetTasks()
        {
            var userId = GetAuthenticatedUserId();
            if (!userId.HasValue)
                // Cambiar el retorno del Unauthorized para que sea más estructurado.
                return Unauthorized(new { message = "Usuario no autenticado." });


            var tareas = await _context.Tareas
                .Where(t => t.UsuarioId == userId)
                .Include(t => t.Usuario)
                .Select(t => new TareaDTO
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    IsCompleted = t.IsCompleted
                })
                .ToListAsync();

            return Ok(tareas);
        }

        [HttpPost]
        public async Task<ActionResult<TareaDTO>> CreateTask([FromBody] TareaDTO tareaDTO)
        {
            var userId = GetAuthenticatedUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Usuario no autenticado." });

            // Validación adicional si es necesario
            if (string.IsNullOrWhiteSpace(tareaDTO.Title))
            {
                return BadRequest(new { message = "El título de la tarea es obligatorio." });
            }
            if (string.IsNullOrWhiteSpace(tareaDTO.Description))
            {
                return BadRequest(new { message = "La descripción de la tarea es obligatoria." });
            }

            // Crear la tarea asociada al usuario autenticado
            var tarea = new Tarea
            {
                Title = tareaDTO.Title,
                Description = tareaDTO.Description,
                IsCompleted = tareaDTO.IsCompleted,
                UsuarioId = userId.Value // Asignar automáticamente el UsuarioId
            };

            _context.Tareas.Add(tarea);
            await _context.SaveChangesAsync();

            // Crear el DTO para la tarea recién creada
            var tareaCreated = new TareaDTO
            {
                Id = tarea.Id,
                Title = tarea.Title,
                Description = tarea.Description,
                IsCompleted = tarea.IsCompleted
                // Usuario se omite aquí, ya no está presente en el DTO
            };

            return Ok(tareaCreated);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TareaDTO>> GetTask(int id)
        {
            var userId = GetAuthenticatedUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Usuario no autenticado." });

            var task = await _context.Tareas
                .Where(t => t.Id == id && t.UsuarioId == userId)
                .Select(t => new TareaDTO
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    IsCompleted = t.IsCompleted
                })
                .FirstOrDefaultAsync();

            if (task == null)
                return NotFound(new { message = "Tarea no encontrada." });

            return Ok(task);
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TareaDTO tareaDTO)
        {
            var userId = GetAuthenticatedUserId();
            if (!userId.HasValue)
                return Unauthorized("Usuario no autenticado.");

            var taskToUpdate = await _context.Tareas.FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == userId);

            if (taskToUpdate == null)
                return NotFound("Tarea no encontrada.");

            taskToUpdate.Title = tareaDTO.Title;
            taskToUpdate.Description = tareaDTO.Description;
            taskToUpdate.IsCompleted = tareaDTO.IsCompleted;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = GetAuthenticatedUserId();
            if (!userId.HasValue)
                return Unauthorized("Usuario no autenticado.");

            var taskToDelete = await _context.Tareas.FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == userId);

            if (taskToDelete == null)
                return NotFound("Tarea no encontrada.");

            _context.Tareas.Remove(taskToDelete);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
