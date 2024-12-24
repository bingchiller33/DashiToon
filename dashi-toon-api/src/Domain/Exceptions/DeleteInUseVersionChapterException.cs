namespace DashiToon.Api.Domain.Exceptions;

public class DeleteInUseVersionChapterException : Exception
{
    public DeleteInUseVersionChapterException() : base("Version cannot be deleted because it is being used.")
    {
    }
}
