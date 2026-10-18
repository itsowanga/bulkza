using Domain;
using Application.Interfaces;

namespace Application.Products.UpdateDescription;

public class UpdateDescriptionHandler(IProductRepository product, IUnitOfWork uow)
{
    private readonly IProductRepository _product = product;
    private readonly IUnitOfWork _uow = uow;

    public async Task<UpdateDescriptionResult> Handle(UpdateDescriptionCommand cmd, CancellationToken ct)
    {
        Product product_ = await _product.GetByIdAsync(cmd.ProductId, ct) ?? throw new KeyNotFoundException("Product is not found.");
        product_.UpdateDescription(cmd.Description);
        _product.Update(product_);
        await _uow.SaveChangesAsync(ct);
        return new(product_.ProductId.Value, product_.Description);
    } 
}