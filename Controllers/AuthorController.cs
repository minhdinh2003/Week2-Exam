using Microsoft.AspNetCore.Mvc;
using QuanLySach.Dto;
using QuanLySach.Models;
using QuanLySach.Repository.Interface;

namespace QuanLySach.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorController(IUnitOfWork unitOfWork, IRedisService cacheService) : ControllerBase
{
    private readonly string _authorListCacheKey = "author_list";

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var cachedAuthors = await cacheService.GetAsync<IEnumerable<Author>>(_authorListCacheKey);
        if (cachedAuthors != null)
            return Ok(cachedAuthors);

        var authors = await unitOfWork.Authors.GetAllAsync();
        await cacheService.SetAsync(_authorListCacheKey, authors, TimeSpan.FromMinutes(1));

        return Ok(authors);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var cacheKey = $"author_{id}";
        var cachedAuthor = await cacheService.GetAsync<Author>(cacheKey);
        if (cachedAuthor != null)
            return Ok(cachedAuthor);

        var author = await unitOfWork.Authors.GetByIdAsync(id);
        if (author == null) return NotFound();

        await cacheService.SetAsync(cacheKey, author, TimeSpan.FromMinutes(1));
        return Ok(author);
    }

    [HttpGet("with-books/{id}")]
    public async Task<IActionResult> GetAuthorWithBooks(int id)
    {
        var cacheKey = $"author_with_books_{id}";
        var cached = await cacheService.GetAsync<Author>(cacheKey);
        if (cached != null)
            return Ok(cached);

        var author = await unitOfWork.Authors.GetAuthorWithBooksAsync(id);
        if (author == null) return NotFound();

        var result = new Author
        {
            Id = author.Id,
            Name = author.Name,
            Books = author.Books.Select(b => new Book
            {
                Id = b.Id,
                Title = b.Title,
                AuthorId = b.AuthorId,
                Rate = b.Rate
            }).ToList()
        };

        await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(1));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Author author)
    {
        await unitOfWork.Authors.AddAsync(author);
        await unitOfWork.SaveChangesAsync();

        // Clear cache
        await cacheService.RemoveAsync(_authorListCacheKey);

        return CreatedAtAction(nameof(GetById), new { id = author.Id }, author);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Author author)
    {
        if (id != author.Id) return BadRequest();

        unitOfWork.Authors.Update(author);
        await unitOfWork.SaveChangesAsync();

        // Clear related cache
        await cacheService.RemoveAsync(_authorListCacheKey);
        await cacheService.RemoveAsync($"author_{id}");
        await cacheService.RemoveAsync($"author_with_books_{id}");

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var author = await unitOfWork.Authors.GetByIdAsync(id);
        if (author == null) return NotFound();

        unitOfWork.Authors.Delete(author);
        await unitOfWork.SaveChangesAsync();

        // Clear related cache
        await cacheService.RemoveAsync(_authorListCacheKey);
        await cacheService.RemoveAsync($"author_{id}");
        await cacheService.RemoveAsync($"author_with_books_{id}");

        return NoContent();
    }
}
