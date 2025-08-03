using Microsoft.AspNetCore.Mvc;
using QuanLySach.Models;
using QuanLySach.Repository.Interface;

namespace QuanLySach.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookController(IUnitOfWork unitOfWork, IRedisService cacheService) : ControllerBase
{
    private readonly string _bookListCacheKey = "book_list";

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var cachedBooks = await cacheService.GetAsync<IEnumerable<Book>>(_bookListCacheKey);
        if (cachedBooks != null)
            return Ok(cachedBooks);

        var books = await unitOfWork.Books.GetAllAsync();
        await cacheService.SetAsync(_bookListCacheKey, books, TimeSpan.FromMinutes(1));

        return Ok(books);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var cacheKey = $"book_{id}";
        var cachedBook = await cacheService.GetAsync<Book>(cacheKey);
        if (cachedBook != null)
            return Ok(cachedBook);

        var book = await unitOfWork.Books.GetByIdAsync(id);
        if (book == null) return NotFound();

        await cacheService.SetAsync(cacheKey, book, TimeSpan.FromMinutes(1));
        return Ok(book);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Book book)
    {
        await unitOfWork.Books.AddAsync(book);
        await unitOfWork.SaveChangesAsync();

        // Clear cache
        await cacheService.RemoveAsync(_bookListCacheKey);

        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Book book)
    {
        if (id != book.Id) return BadRequest();

        unitOfWork.Books.Update(book);
        await unitOfWork.SaveChangesAsync();

        // Clear related cache
        await cacheService.RemoveAsync(_bookListCacheKey);
        await cacheService.RemoveAsync($"book_{id}");

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var book = await unitOfWork.Books.GetByIdAsync(id);
        if (book == null) return NotFound();

        await unitOfWork.Books.DeleteBookAndReviewsAsync(id);
        await unitOfWork.SaveChangesAsync();

        // Clear related cache
        await cacheService.RemoveAsync(_bookListCacheKey);
        await cacheService.RemoveAsync($"book_{id}");

        return NoContent();
    }
}
