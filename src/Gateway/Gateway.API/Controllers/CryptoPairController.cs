using Gateway.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gateway.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CryptoPairController : ControllerBase
{
    private readonly AppDbContext _db;

    public CryptoPairController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetPairs()
    {
        var pairs = await _db.CryptoPairs
            .Where(p => p.IsActive)
            .OrderBy(p => p.Symbol)
            .ToListAsync();
        return Ok(pairs);
    }
}
