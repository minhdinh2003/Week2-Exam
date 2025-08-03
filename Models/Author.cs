using System.ComponentModel.DataAnnotations;

namespace QuanLySach.Models
{
    public class Author
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string? Name { get; set; }
        public ICollection<Book> Books { get; set; } = [];
    }
}
