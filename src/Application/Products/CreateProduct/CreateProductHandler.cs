using Domain;
using Application.Interfaces;

namespace Application.Products.CreateProduct;

public class CreateProductHandler(IProductRepository product, IUnitOfWork uow)
{
    private readonly IProductRepository _product = product;
    private readonly IUnitOfWork _uow = uow;

    public async Task<CreateProductResult> Handle(CreateProductCommand cmd, CancellationToken ct)
    {
        Product product_ = new(cmd.ProductId, cmd.Sku, cmd.Name, cmd.Description, cmd.ProductCategory, cmd.UnitOfMeasure,
                               cmd.BasePrice, cmd.IsActive);
        await _product.AddAsync(product_, ct);
        await _uow.SaveChangesAsync(ct);
        return new(product_.ProductId, product_.Name, product_.SKU);
    } 
}