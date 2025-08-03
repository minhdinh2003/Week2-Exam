using System;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using QuanLySach.Data;
using QuanLySach.Models;
using QuanLySach.MongoModels;
using QuanLySach.Repository.Interface;

namespace QuanLySach.Repository;

public class ReviewRepository : IReviewRepository
{
    private readonly IMongoCollection<Review> _collection;

    public ReviewRepository(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var db = client.GetDatabase(settings.Value.Database);
        _collection = db.GetCollection<Review>("Reviews");
    }

    public Task<List<Review>> GetByBookIdAsync(int bookId) =>
        _collection.Find(r => r.Book.Id == bookId).ToListAsync();

    public Task<List<Review>> GetByReviewerIdAsync(int reviewerId) =>
        _collection.Find(r => r.Reviewer.Id == reviewerId).ToListAsync();

    public Task AddAsync(Review review) => _collection.InsertOneAsync(review);
    

}
