namespace QuanLySach.Dto
{
    public class ReviewerDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public ICollection<ReviewDto> Reviews { get; set; } = [];
    }
}
