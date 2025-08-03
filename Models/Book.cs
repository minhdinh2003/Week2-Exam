using System.ComponentModel.DataAnnotations;
using QuanLySach.MongoModels;

namespace QuanLySach.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public required string Title { get; set; }
        public int AuthorId { get; set; }
        public Author? Author { get; set; }
        public double Rate { get; set; }
        public ICollection<Review> Reviews { get; set; } = [];
    }
}
