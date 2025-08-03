using QuanLySach.Models;

namespace QuanLySach.Repository.Interface;

public interface IAuthorRepository : IGenericRepository<Author>
{
    Task<Author?> GetAuthorWithBooksAsync(int id);
}
