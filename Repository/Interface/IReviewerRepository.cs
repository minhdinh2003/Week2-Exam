using QuanLySach.Models;

namespace QuanLySach.Repository.Interface;

public interface IReviewerRepository : IGenericRepository<Reviewer>
{
    Task<Reviewer?> GetReviewsAsync(int id);
}
