namespace DashiToon.Api.Domain.Exceptions;

public class AlreadyReviewedException : Exception
{
    public AlreadyReviewedException() : base($"User can only have one review per series")
    {
    }
}
