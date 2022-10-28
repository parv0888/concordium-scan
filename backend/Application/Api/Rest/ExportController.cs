﻿using System.Threading.Tasks;
using System.Text;
using Application.Api.GraphQL.Accounts;
using Application.Api.GraphQL.EfCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application.Api.Rest;

[ApiController]
public class ExportController : ControllerBase
{
    private readonly IDbContextFactory<GraphQlDbContext> _dbContextFactory;

    public ExportController(IDbContextFactory<GraphQlDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    [HttpGet]
    [Route("rest/export/statement")]
    public async Task<ActionResult> GetStatementExport(string accountAddress)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        if (!ConcordiumSdk.Types.AccountAddress.TryParse(accountAddress, out var parsed))
        {
            return BadRequest("invalid account format");
        }

        var baseAddress = new AccountAddress(parsed!.GetBaseAddress().AsString);
        var account = dbContext.Accounts
            .AsNoTracking()
            .SingleOrDefault(account => account.BaseAddress == baseAddress);
        if (account == null)
        {
            return NotFound("account does not exist");
        }

        var query = dbContext.AccountStatementEntries
            .AsNoTracking()
            .Where(x => x.AccountId == account.Id);

        var scalarQuery = query.Select(x => new
        {
            x.Timestamp,
            x.EntryType,
            x.Amount,
        });
        var values = await scalarQuery.ToListAsync();
        // TODO Should use something like 'CsvHelper' (see 'https://joshclose.github.io/CsvHelper/examples/writing/write-anonymous-type-objects/')?
        var csv = new StringBuilder("Time,Amount (CCD),Label\n");
        foreach (var v in values)
        {
            csv.Append($"{v.Timestamp.ToString("u")},{v.Amount / 1e6},{v.EntryType}\n");
        }

        return new FileContentResult(Encoding.ASCII.GetBytes(csv.ToString()), "text/csv")
        {
            FileDownloadName = $"statement-{accountAddress}.csv",
        };
    }
}