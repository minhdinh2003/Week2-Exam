using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using QuanLySach.Data;
using QuanLySach.Models;
using QuanLySach.MongoModels;
using QuanLySach.Repository.Interface;

namespace QuanLySach.Repository;

public class BookRepository(AppDbContext context, IMongoCollection<Review> mongoReviewCollection)
    : GenericRepository<Book>(context), IBookRepository
{
    private readonly AppDbContext _context = context;
    private readonly IMongoCollection<Review> _mongoReviewCollection = mongoReviewCollection;

    public async Task<Book?> GetBookWithAuthorAsync(int id)
    {
        return await _context.Books
            .Include(b => b.Author)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Book?> GetBookWithReviewsAsync(int id)
    {
        var book = await _context.Books
            .Include(b => b.Author)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
            return null;

        var reviews = await _mongoReviewCollection
            .Find(r => r.BookId == id)
            .ToListAsync();

        book.Reviews = reviews;

        return book;
    }
    public async Task<bool> DeleteBookAndReviewsAsync(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return false;

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();



            await transaction.CommitAsync();
            await _mongoReviewCollection.DeleteManyAsync(r => r.BookId == id);
            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
