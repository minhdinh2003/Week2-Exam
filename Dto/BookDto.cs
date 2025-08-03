namespace QuanLySach.Dto
{
    public class BookDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public int AuthorId { get; set; }
        public double Rate { get; set; }
        public ICollection<ReviewDto> Reviews { get; set; } = [];
    }
}
