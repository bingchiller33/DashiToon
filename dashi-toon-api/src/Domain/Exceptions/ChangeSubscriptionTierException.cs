namespace DashiToon.Api.Domain.Exceptions;

public class ChangeSubscriptionTierException : Exception
{
    public ChangeSubscriptionTierException() : base("Not allowed to change subscription tier")
    {
    }
}
