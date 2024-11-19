namespace TodoListApp.Models.DTOs
{
    public class TareaDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        //public UsuarioDTO Usuario { get; set; }
    }
}
