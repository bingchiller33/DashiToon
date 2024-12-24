using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Services;
using DashiToon.Api.Domain.ValueObjects;

namespace DashiToon.Api.Application.AuthorStudio.Revenue.Commands.WithdrawRevenue;

[Authorize]
public sealed record WithdrawKanaRevenueCommand(string PaypalAccountId, decimal Amount) : IRequest;

public sealed class WithdrawKanaRevenueCommandHandler : IRequestHandler<WithdrawKanaRevenueCommand>
{
    private readonly RevenueService _revenueService;
    private readonly IPaymentService _paymentService;
    private readonly IUserRepository _userRepository;
    private readonly IUser _user;

    public WithdrawKanaRevenueCommandHandler(
        IPaymentService paymentService,
        IUserRepository userRepository,
        IUser user)
    {
        _paymentService = paymentService;
        _revenueService = new RevenueService();
        _userRepository = userRepository;
        _user = user;
    }

    public async Task Handle(WithdrawKanaRevenueCommand request, CancellationToken cancellationToken)
    {
        IDomainUser? user = await _userRepository.GetById(_user.Id!);

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }

        _revenueService.WithdrawRevenue(user, request.Amount);

        await _paymentService.PayoutRevenue(
            request.PaypalAccountId,
            Price.CreateNew(request.Amount, "VND"));

        await _userRepository.Update(user);
    }
}
