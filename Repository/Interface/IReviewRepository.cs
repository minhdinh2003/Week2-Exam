using System;
using QuanLySach.Models;
using QuanLySach.MongoModels;

namespace QuanLySach.Repository.Interface;

public interface IReviewRepository
{
    Task<List<Review>> GetByBookIdAsync(int bookId);
    Task<List<Review>> GetByReviewerIdAsync(int reviewerId);
    Task AddAsync(Review review);
}
