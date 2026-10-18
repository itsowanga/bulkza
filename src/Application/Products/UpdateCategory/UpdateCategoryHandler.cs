using Domain;
using Application.Interfaces;

namespace Application.Products.UpdateCategory;

public class UpdateCategoryHandler(IProductRepository product, IUnitOfWork uow)
{
    private readonly IProductRepository _product = product;
    private readonly IUnitOfWork _uow = uow;

    public async Task<UpdateCategoryResult> Handle(UpdateCategoryCommand cmd, CancellationToken ct)
    {
        Product product_ = await _product.GetByIdAsync(cmd.ProductId, ct) ?? throw new KeyNotFoundException("Product is not found.");
        product_.UpdateCategory(cmd.Category);
        _product.Update(product_);
        await _uow.SaveChangesAsync(ct);
        return new(product_.ProductId.Value, product_.Category);
    } 
}