using Application.BulkPools.CancelPool;
using Application.BulkPools.CreatePool;
using Application.BulkPools.ExpirePool;
using Application.BulkPools.FinalisePool;
using Application.BulkPools.GetPool;
using Application.BulkPools.JoinPool;
using Application.BulkPools.LeavePool;
using Application.BulkPools.LockPool;
using Application.BulkPools.SearchPool.SearchByMerchant;
using Application.BulkPools.SearchPool.SearchOpenByProductId;
using Application.Carts.AddCartItem;
using Application.Carts.ClearCart;
using Application.Carts.RemoveCartItem;
using Application.Carts.UpdateItemQuantity;
using Application.Carts.ViewCart;
using Application.Merchants.ChangeMerchantStatus;
using Application.Merchants.LoginMerchant;
using Application.Merchants.RegisterMerchant;
using Application.Merchants.UpdateMerchantAddress;
using Application.Merchants.UpdateMerchantName;
using Application.Merchants.UpdateMerchantPhoneNumber;
using Application.Orders.CancelOrder;
using Application.Orders.CreateOrder;
using Application.Orders.MarkAsDelivered;
using Application.Orders.MarkAsShipped;
using Application.Orders.SearchOrders.FindAllOrderByMerchant;
using Application.PaymentGateway.ProcessPayment;
using Application.Products.ActivateProduct;
using Application.Products.CreateProduct;
using Application.Products.DeactivateProduct;
using Application.Products.GetProduct;
using Application.Products.SearchProduct.BrowseActiveProducts;
using Application.Products.SearchProduct.SearchProductByName;
using Application.Products.SearchProduct.SearchProductBySku;
using Application.Products.UpdateBasePrice;
using Application.Products.UpdateCategory;
using Application.Products.UpdateDescription;
using Application.Products.UpdateUnitOfMeasure;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // BulkPool handlers
        services.AddScoped<CancelPoolHandler>();
        services.AddScoped<CreatePoolHandler>();
        services.AddScoped<ExpirePoolHandler>();
        services.AddScoped<FinalisePoolHandler>();
        services.AddScoped<GetPoolHandler>();
        services.AddScoped<JoinPoolHandler>();
        services.AddScoped<LeavePoolHandler>();
        services.AddScoped<LockPoolHandler>();
        services.AddScoped<SearchByMerchantHandler>();
        services.AddScoped<SearchOpenByProductIdHandler>();

        // Cart handlers
        services.AddScoped<AddCartItemHandler>();
        services.AddScoped<ClearCartHandler>();
        services.AddScoped<RemoveCartItemHandler>();
        services.AddScoped<UpdateItemQuantityHandler>();
        services.AddScoped<ViewCartHandler>();

        // Merchant handlers
        services.AddScoped<ChangeMerchantStatusHandler>();
        services.AddScoped<RegisterMerchantHandler>();
        services.AddScoped<LoginMerchantHandler>();
        services.AddScoped<UpdateMerchantAddressHandler>();
        services.AddScoped<UpdateMerchantNameHandler>();
        services.AddScoped<UpdateMerchantPhoneNumberHandler>();


        // Order handlers
        services.AddScoped<CancelOrderHandler>();
        services.AddScoped<CreateOrderHandler>();
        services.AddScoped<MarkAsDeliveredHandler>();
        services.AddScoped<MarkAsShippedHandler>();
        services.AddScoped<FindAllOrderByMerchantHandler>();

        // Payment gateway handler
        services.AddScoped<ProcessPaymentHandler>();

        // Product handler
        services.AddScoped<ActivateProductHandler>();
        services.AddScoped<CreateProductHandler>();
        services.AddScoped<DeactivateProductHandler>();
        services.AddScoped<GetProductHandler>();
        services.AddScoped<UpdateBasePriceHandler>();
        services.AddScoped<UpdateCategoryHandler>();
        services.AddScoped<UpdateDescriptionHandler>();
        services.AddScoped<UpdateUnitOfMeasureHandler>();
        services.AddScoped<BrowseActiveProductsHandler>();
        services.AddScoped<SearchProductByNameHandler>();
        services.AddScoped<SearchProductBySkuHandler>();

        return services;
    }
}