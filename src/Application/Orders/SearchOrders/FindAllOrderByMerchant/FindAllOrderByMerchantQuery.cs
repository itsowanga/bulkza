using Domain.ValueObjects;

namespace Application.Orders.SearchOrders.FindAllOrderByMerchant;

public record FindAllOrderByMerchantQuery(MerchantId MerchantId, int Page, int PageSize);