using System.ComponentModel.DataAnnotations;
using QuanLySach.MongoModels;

namespace QuanLySach.Models
{
    public class Reviewer
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string? Name { get; set; }
        public ICollection<Review> Reviews { get; set; } = [];
    }
}
