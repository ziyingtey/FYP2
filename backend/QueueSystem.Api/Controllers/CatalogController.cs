using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QueueSystem.Api.Data;
using QueueSystem.Api.Dtos;

namespace QueueSystem.Api.Controllers;

[ApiController]
[Route("api/catalog")]
[AllowAnonymous]
public class CatalogController : ControllerBase
{
    private readonly AppDbContext _db;

    public CatalogController(AppDbContext db) => _db = db;

    [HttpGet("services")]
    public async Task<ActionResult<IReadOnlyList<ServiceCatalogDto>>> Services(CancellationToken ct)
    {
        var rows = await _db.BankServices.AsNoTracking()
            .OrderBy(s => s.Id)
            .Select(s => new ServiceCatalogDto(s.Id, s.Code, s.DisplayName, s.AvgServiceTimeMinutes))
            .ToListAsync(ct);
        return Ok(rows);
    }
}
