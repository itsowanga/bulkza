using Application.Interfaces;
using Domain;

namespace Application.Products.DeactivateProduct;

public class DeactivateProductHandler(IProductRepository product, IUnitOfWork uow)
{
    private readonly IProductRepository _product = product;
    private readonly IUnitOfWork _uow = uow;

    public async Task<DeactivateProductResult> Handle(DeactivateProductCommand cmd, CancellationToken ct)
    {
        Product product_ = await _product.GetByIdAsync(cmd.ProductId, ct) ?? throw new KeyNotFoundException("Product is not found.");
        product_.Deactivate();
        _product.Update(product_);
        await _uow.SaveChangesAsync(ct);
        return new(product_.ProductId, product_.IsActive);
    } 
}