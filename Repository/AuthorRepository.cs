using Microsoft.EntityFrameworkCore;
using QuanLySach.Data;
using QuanLySach.Models;
using QuanLySach.Repository.Interface;

namespace QuanLySach.Repository;

public class AuthorRepository(AppDbContext context) : GenericRepository<Author>(context), IAuthorRepository
{
    private readonly AppDbContext _context = context;

    public async Task<Author?> GetAuthorWithBooksAsync(int id)
    {
        return await _context.Authors
            .Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
}
