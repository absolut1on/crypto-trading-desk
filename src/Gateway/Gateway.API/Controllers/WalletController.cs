using Gateway.API.Data;
using Gateway.API.Entities;
using Gateway.API.Sagas;
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
    private readonly TradeSaga _saga;

    public WalletController(AppDbContext db, TradeSaga saga)
    {
        _db = db;
        _saga = saga;
    }

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

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] WalletRequest request)
    {
        if (request.Amount <= 0)
            return BadRequest(new { error = "Amount must be positive" });

        try
        {
            await _saga.DepositAsync(request.UserId, request.Currency, request.Amount);
            return Ok(new { message = $"Deposited {request.Amount} {request.Currency}" });
        }
        catch (SagaException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WalletRequest request)
    {
        if (request.Amount <= 0)
            return BadRequest(new { error = "Amount must be positive" });

        try
        {
            await _saga.WithdrawAsync(request.UserId, request.Currency, request.Amount);
            return Ok(new { message = $"Withdrew {request.Amount} {request.Currency}" });
        }
        catch (SagaException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }
}

public record WalletRequest(Guid UserId, string Currency, decimal Amount);
