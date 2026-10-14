using Application.Interfaces;
using Domain;

namespace Application.Products.ActivateProduct;

public class ActivateProductHandler(IProductRepository product, IUnitOfWork uow)
{
    private readonly IProductRepository _product = product;
    private readonly IUnitOfWork _uow = uow;

    public async Task<ActivateProductResult> Handle(ActivateProductCommand cmd, CancellationToken ct)
    {
        Product product_ = await _product.GetByIdAsync(cmd.ProductId, ct) ?? throw new KeyNotFoundException("Product is not found.");
        product_.Activate();
        _product.Update(product_);
        await _uow.SaveChangesAsync(ct);
        return new(product_.ProductId, product_.IsActive);
    } 
}