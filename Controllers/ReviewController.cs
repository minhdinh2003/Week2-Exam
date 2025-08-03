using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLySach.Data;
using QuanLySach.Dto;
using QuanLySach.MongoModels;
using QuanLySach.Repository.Interface;

namespace QuanLySach.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewController(
    AppDbContext context,
    IReviewRepository reviewRepository,
    IRedisService cacheService
) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IReviewRepository _reviewRepository = reviewRepository;

    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] ReviewDto reviewDto)
    {
        var book = await _context.Books.FindAsync(reviewDto.BookId);
        var reviewer = await _context.Reviewers.FindAsync(reviewDto.ReviewerId);
        if (book == null || reviewer == null)
            return NotFound("Book hoặc Reviewer không tồn tại");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var review = new Review
            {
                Name = reviewDto.Name ?? "No title",
                Description = reviewDto.Description ?? "No description",
                Rate = reviewDto.Rate,
                BookId = reviewDto.BookId,
                ReviewerId = reviewDto.ReviewerId,
                Book = new BookDto { Id = book.Id, Title = book.Title },
                Reviewer = new ReviewerDto { Id = reviewer.Id, Name = reviewer.Name }
            };

            var reviews = await _reviewRepository.GetByBookIdAsync(book.Id);
            if (reviews.Count > 0)
            {
                book.Rate = (float)((reviews.Sum(r => r.Rate) + review.Rate) / (reviews.Count + 1));
                _context.Books.Update(book);
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            await _reviewRepository.AddAsync(review);

            // Clear cache
            await cacheService.RemoveAsync($"reviews_book_{review.BookId}");
            await cacheService.RemoveAsync($"reviews_reviewer_{review.ReviewerId}");

            return Ok("Review đã được thêm và cập nhật rate của Book.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Đã xảy ra lỗi: {ex.Message}");
        }
    }

    [HttpGet("book/{bookId}")]
    public async Task<IActionResult> GetByBookId(int bookId)
    {
        var cacheKey = $"reviews_book_{bookId}";
        var cached = await cacheService.GetAsync<IEnumerable<Review>>(cacheKey);
        if (cached != null)
        {
            var resultCached = cached.Select(r => new
            {
                r.Id,
                r.Name,
                r.Description,
                r.Rate,
                Book = r.Book,
                Reviewer = r.Reviewer
            });
            return Ok(resultCached);
        }

        var reviews = await _reviewRepository.GetByBookIdAsync(bookId);

        var result = reviews.Select(r => new
        {
            r.Id,
            r.Name,
            r.Description,
            r.Rate,
            Book = new BookDto
            {
                Id = r.Book.Id,
                Title = r.Book.Title,
                AuthorId = r.Book.AuthorId,
                Rate = r.Book.Rate
            },
            Reviewer = new ReviewerDto
            {
                Id = r.Reviewer.Id,
                Name = r.Reviewer.Name
            }
        });

        await cacheService.SetAsync(cacheKey, reviews, TimeSpan.FromMinutes(10));
        return Ok(result);
    }

    [HttpGet("reviewer/{reviewerId}")]
    public async Task<IActionResult> GetByReviewerId(int reviewerId)
    {
        var cacheKey = $"reviews_reviewer_{reviewerId}";
        var cached = await cacheService.GetAsync<IEnumerable<Review>>(cacheKey);
        if (cached != null)
        {
            var resultCached = cached.Select(r => new
            {
                r.Id,
                r.Name,
                r.Description,
                r.Rate,
                Book = r.Book,
                Reviewer = r.Reviewer
            });
            return Ok(resultCached);
        }

        var reviews = await _reviewRepository.GetByReviewerIdAsync(reviewerId);

        var result = reviews.Select(r => new
        {
            r.Id,
            r.Name,
            r.Description,
            r.Rate,
            Book = new BookDto
            {
                Id = r.Book.Id,
                Title = r.Book.Title,
                AuthorId = r.Book.AuthorId,
                Rate = r.Book.Rate
            },
            Reviewer = new ReviewerDto
            {
                Id = r.Reviewer.Id,
                Name = r.Reviewer.Name
            }
        });

        await cacheService.SetAsync(cacheKey, reviews, TimeSpan.FromMinutes(10));
        return Ok(result);
    }
}
