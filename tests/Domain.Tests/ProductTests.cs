using Xunit;
using FluentAssertions;
using Domain.Tests.Builders;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Tests;

public class ProductTests
{
    [Fact]
    public void
    Constructor_WhenProductIdIsTheDefault_ShouldThrowArgumentException()
    {
        ProductBuilder product = new();
        Action act = () => product.WithProductId(default).Build();
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Constructor_WhenProductCategoryIsNotDefined_ShouldThrowArgumentException()
    {
        ProductBuilder product = new();
        Action act = () => product.WithProductCategory((ProductCategory)999).Build();
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Constructor_WhenUnitOfMeasureIsNotDefined_ShouldThrowArgumentException()
    {
        ProductBuilder product = new();
        Action act = () => product.WithUnitOfMeasure((UnitOfMeasure)999).Build();
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Constructor_WhenSkuIsNullOrWhitespace_ShouldThrowArgumentException()
    {
        ProductBuilder product = new();
        Action act = () => product.WithSku(null!).Build();
        act.Should().Throw<ArgumentException>();
    }  

    [Fact]
    public void
    Constructor_WhenSkuExceedsTheCharacterLimit_ShouldThrowArgumentException()
    {
        ProductBuilder product = new();
        string chars = "abcdefgh";
        string randomString = new([.. Enumerable.Range(0, 59).Select(_ => chars[Random.Shared.Next(chars.Length)])]);
        Action act = () => product.WithSku(randomString).Build();
        act.Should().Throw<ArgumentException>();
    }  

    [Fact]
    public void
    Constructor_WhenNameIsNullOrWhitespace_ShouldThrowArgumentException()
    {
        ProductBuilder product = new();
        Action act = () => product.WithName(null!).Build();
        act.Should().Throw<ArgumentException>();
    }  

    [Fact]
    public void
    Constructor_WhenNameExceedsTheCharacterLimit_ShouldThrowArgumentException()
    {
        ProductBuilder product = new();
        string chars = "abcdefgh";
        string randomString = new([.. Enumerable.Range(0, 109).Select(_ => chars[Random.Shared.Next(chars.Length)])]);
        Action act = () => product.WithName(randomString).Build();
        act.Should().Throw<ArgumentException>();
    }  

    [Fact]
    public void
    Constructor_WhenDescriptionExceedsCharacterLimit_ShouldBeUpdated()
    {
        ProductBuilder product = new();
        string chars = "abcdefgh";
        string randomString = new([.. Enumerable.Range(0, 509).Select(_ => chars[Random.Shared.Next(chars.Length)])]);
        Action act = () => product.WithProductDescription(randomString).Build();
        act.Should().Throw<ArgumentException>();
    }  

    [Fact]
    public void
    Method_WhenUpdatingDescription_ShouldBeUpdated()
    {
        ProductBuilder product = new();
        Product p = product.Build();
        p.UpdateDescription("abcdefgh");
        p.Description.Should().Be("abcdefgh");
    }  

    [Fact]
    public void
    Method_WhenUpdatingCategory_ShouldBeUpdated()
    {
        ProductBuilder product = new();
        Product p = product.Build();
        p.UpdateCategory(ProductCategory.Flour);
        p.Category.Should().Be(ProductCategory.Flour);
    }  

    [Fact]
    public void
    Method_WhenUpdatingUnitOfMeasure_ShouldBeUpdated()
    {
        ProductBuilder product = new();
        Product p = product.Build();
        p.UpdateUnitOfMeasure(UnitOfMeasure.Each);
        p.UnitOfMeasure.Should().Be(UnitOfMeasure.Each);
    }  

    [Fact]
    public void
    Method_WhenUpdatingBasePrice_ShouldBeUpdated()
    {
        ProductBuilder product = new();
        Product p = product.Build();
        p.UpdateBasePrice(Money.Create(555m));
        p.BasePrice.Should().Be(Money.Create(555m));
    } 

    [Fact]
    public void
    Method_WhenActivatingProduct_ShouldBeUpdated()
    {
        ProductBuilder product = new();
        Product p = product.Build();
        p.Activate();
        p.IsActive.Should().Be(true);
    }        

    [Fact]
    public void
    Method_WhenDeactivatingProduct_ShouldBeUpdated()
    {
        ProductBuilder product = new();
        Product p = product.Build();
        p.Deactivate();
        p.IsActive.Should().Be(false);
    }     
}