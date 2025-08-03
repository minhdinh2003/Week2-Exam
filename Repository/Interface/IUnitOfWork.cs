using System;
using QuanLySach.Models;

namespace QuanLySach.Repository.Interface;
public interface IUnitOfWork : IDisposable
{
    IBookRepository Books { get; }
    IReviewerRepository Reviewers { get; }
    IAuthorRepository Authors { get; }
    IQueryable<Book> BooksQueryable { get; }
    IQueryable<Reviewer> ReviewersQueryable { get; }
    IQueryable<Author> AuthorsQueryable { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
