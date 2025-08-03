using Microsoft.AspNetCore.Mvc;
using QuanLySach.Models;
using QuanLySach.Repository.Interface;
using StackExchange.Redis;
using System.Text.Json;

namespace QuanLySach.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewerController(IUnitOfWork unitOfWork, IConnectionMultiplexer redis) : ControllerBase
{
    private readonly IDatabase _cache = redis.GetDatabase();
    private const string CacheKey = "reviewers";

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var cachedData = await _cache.StringGetAsync(CacheKey);
        if (cachedData.HasValue)
        {
            var reviewers = JsonSerializer.Deserialize<IEnumerable<Reviewer>>(cachedData!);
            return Ok(reviewers);
        }

        var reviewersFromDb = await unitOfWork.Reviewers.GetAllAsync();
        var serialized = JsonSerializer.Serialize(reviewersFromDb);
        await _cache.StringSetAsync(CacheKey, serialized, TimeSpan.FromMinutes(1)); // Cache 5 phút
        return Ok(reviewersFromDb);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        string cacheKey = $"reviewer:{id}";

        var cachedData = await _cache.StringGetAsync(cacheKey);
        if (cachedData.HasValue)
        {
            var reviewer = JsonSerializer.Deserialize<Reviewer>(cachedData!);
            return Ok(reviewer);
        }

        var reviewerFromDb = await unitOfWork.Reviewers.GetByIdAsync(id);
        if (reviewerFromDb == null) return NotFound();

        var serialized = JsonSerializer.Serialize(reviewerFromDb);
        await _cache.StringSetAsync(cacheKey, serialized, TimeSpan.FromMinutes(1));

        return Ok(reviewerFromDb);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Reviewer reviewer)
    {
        await unitOfWork.Reviewers.AddAsync(reviewer);
        await unitOfWork.SaveChangesAsync();
        await _cache.KeyDeleteAsync(CacheKey); // Xoá cache để làm mới
        return CreatedAtAction(nameof(GetById), new { id = reviewer.Id }, reviewer);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Reviewer reviewer)
    {
        if (id != reviewer.Id) return BadRequest();
        unitOfWork.Reviewers.Update(reviewer);
        await unitOfWork.SaveChangesAsync();
        await _cache.KeyDeleteAsync(CacheKey); // Xoá cache
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var reviewer = await unitOfWork.Reviewers.GetByIdAsync(id);
        if (reviewer == null) return NotFound();
        unitOfWork.Reviewers.Delete(reviewer);
        await unitOfWork.SaveChangesAsync();
        await _cache.KeyDeleteAsync(CacheKey); // Xoá cache
        return NoContent();
    }
}
