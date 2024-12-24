namespace DashiToon.Api.Domain.Exceptions;

public class PurchaseOrderNotFoundException : Exception
{
    public PurchaseOrderNotFoundException() : base("Order Not Found")
    {
    }
}
