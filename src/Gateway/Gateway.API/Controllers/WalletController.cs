using Gateway.API.Data;
using Gateway.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gateway.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly AppDbContext _db;

    public WalletController(AppDbContext db) => _db = db;

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetWallets(Guid userId)
    {
        var wallets = await _db.Wallets
            .Where(w => w.UserId == userId)
            .ToListAsync();
        return Ok(wallets);
    }

    [HttpGet("{userId:guid}/balances")]
    public async Task<IActionResult> GetBalances(Guid userId)
    {
        // Will read from wallet_balances materialized view in Phase 3
        var balances = await _db.WalletEvents
            .Include(we => we.Wallet)
            .Where(we => we.Wallet.UserId == userId)
            .GroupBy(we => we.Wallet.Currency)
            .Select(g => new
            {
                Currency = g.Key,
                Balance = g.OrderByDescending(we => we.Timestamp).First().BalanceAfter
            })
            .ToListAsync();

        return Ok(balances);
    }
}
