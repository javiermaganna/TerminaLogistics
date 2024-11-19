using System.Threading;

namespace TodoListApp.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public List<Tarea> Tareas { get; set; }
    }
}
