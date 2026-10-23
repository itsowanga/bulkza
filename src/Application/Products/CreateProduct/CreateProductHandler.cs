using Domain;
using Application.Interfaces;
using Domain.ValueObjects;

namespace Application.Products.CreateProduct;

public class CreateProductHandler(IProductRepository product, IUnitOfWork uow)
{
    private readonly IProductRepository _product = product;
    private readonly IUnitOfWork _uow = uow;

    public async Task<CreateProductResult> Handle(CreateProductCommand cmd, CancellationToken ct)
    {
        Product product_ = new(ProductId.New(), Guid.NewGuid().ToString("N")[..15], cmd.Name, cmd.Description, cmd.ProductCategory, cmd.UnitOfMeasure,
                               cmd.BasePrice, true);
        await _product.AddAsync(product_, ct);
        await _uow.SaveChangesAsync(ct);
        return new(product_.ProductId.Value, product_.Name, product_.SKU);
    } 
}