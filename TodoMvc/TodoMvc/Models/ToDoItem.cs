using System;
using System.ComponentModel.DataAnnotations;

namespace TodoMvc.Models
{
    public class ToDoItem
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        public bool IsDone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
