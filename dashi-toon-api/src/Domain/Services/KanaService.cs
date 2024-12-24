namespace DashiToon.Api.Domain.Services;

public static class KanaService
{
    public static PurchaseOrder CreatePurchaseOrder(string orderId, IDomainUser user, KanaGoldPack pack)
    {
        if (!pack.IsActive)
        {
            throw new KanaGoldPackInactiveException();
        }

        return PurchaseOrder.CreateNew(
            orderId,
            user.Id,
            pack.Id,
            pack.Price
        );
    }

    public static void CompleteOrder(IDomainUser user, PurchaseOrder order, KanaGoldPack purchasePack)
    {
        order.CompleteOrder();

        user.AddTransaction(KanaTransaction.Create(
            KanaType.Gold,
            TransactionType.TopUp,
            purchasePack.Amount,
            $"Mua gói {purchasePack.Amount} Kana Gold"
        ));
    }
}
