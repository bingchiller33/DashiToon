namespace DashiToon.Api.Domain.Exceptions;

public class VolumeNotFoundException : Exception
{
    public VolumeNotFoundException() : base("Volume not found")
    {
    }
}
