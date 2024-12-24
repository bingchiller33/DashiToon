using DashiToon.Api.Application.Administrator.KanaGoldPacks.Models;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace DashiToon.Api.Application.Administrator.KanaGoldPacks.Queries.GetKanaGoldPacks;

[Authorize(Roles = Roles.Administrator)]
public sealed record GetKanaGoldPackQuery : IRequest<List<KanaGoldPackVm>>;

public sealed class GetKanaGoldPackQueryHandler
    : IRequestHandler<GetKanaGoldPackQuery, List<KanaGoldPackVm>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrencyService _currencyService;

    public GetKanaGoldPackQueryHandler(
        IApplicationDbContext dbContext,
        ICurrencyService currencyService)
    {
        _dbContext = dbContext;
        _currencyService = currencyService;
    }

    public async Task<List<KanaGoldPackVm>> Handle(
        GetKanaGoldPackQuery request,
        CancellationToken cancellationToken)
    {
        List<KanaGoldPackVm>? result = await _dbContext.KanaGoldPacks
            .AsNoTracking()
            .Select(pack => new KanaGoldPackVm(
                pack.Id,
                pack.Amount,
                pack.Price,
                pack.IsActive
            ))
            .ToListAsync(cancellationToken);

        result.ForEach(x => x = x with { Price = _currencyService.ConvertPrice(x.Price, "VND") });

        return result;
    }
}
