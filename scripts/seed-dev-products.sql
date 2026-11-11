-- Sample products for local development. Safe to run more than once: a product is only inserted when its SKU is new.
-- The API has no "create product" route yet, so this is the way to get products into a fresh database.
--
-- Every product gets the standard volume ladder the domain adds to new products:
-- 1+ units 0%, 50+ 5%, 100+ 10%, 250+ 15%, 500+ 20%, 1000+ 25%.

DECLARE @tiers nvarchar(max) = N'[{"MinimumQuantity":1,"DiscountPercentage":0},{"MinimumQuantity":50,"DiscountPercentage":5},{"MinimumQuantity":100,"DiscountPercentage":10},{"MinimumQuantity":250,"DiscountPercentage":15},{"MinimumQuantity":500,"DiscountPercentage":20},{"MinimumQuantity":1000,"DiscountPercentage":25}]';

DECLARE @products TABLE (Sku nvarchar(50), Name nvarchar(100), Description nvarchar(500), Category nvarchar(11), Unit nvarchar(9), Price decimal(18, 2));

INSERT INTO @products (Sku, Name, Description, Category, Unit, Price) VALUES
  (N'SKU-MAIZE-WS-10KG',  N'White Star Maize Meal 10 kg',  N'Super maize meal, 10 kg bag.',         N'MaizeMeal', N'Bag',      89.99),
  (N'SKU-MAIZE-IM-12-5KG', N'Ace Maize Meal 12.5 kg',       N'Super maize meal, 12.5 kg bag.',       N'MaizeMeal', N'Bag',     109.50),
  (N'SKU-OIL-SUN-5LTR-01', N'Sunflower Cooking Oil 5 litre', N'Refined sunflower oil, 5 litre.',      N'CookingOil', N'Litre',  129.00),
  (N'SKU-OIL-SUN-750ML-01', N'Sunflower Cooking Oil 750 ml', N'Refined sunflower oil, 750 ml bottle.', N'CookingOil', N'Each',    28.99),
  (N'SKU-RICE-LONG-10KG-1', N'Long Grain Rice 10 kg',        N'Parboiled long grain rice, 10 kg bag.', N'Rice',     N'Bag',     149.00),
  (N'SKU-SUGAR-WHT-10KG-1', N'White Sugar 10 kg',            N'Refined white sugar, 10 kg bag.',       N'Sugar',    N'Bag',     179.99),
  (N'SKU-DRINK-COLA-24X300', N'Cola 300 ml, case of 24',     N'Cola in 300 ml cans, case of 24.',      N'SoftDrinks', N'Box',   189.00),
  (N'SKU-FLOUR-CAKE-12-5KG', N'Cake Flour 12.5 kg',          N'Cake wheat flour, 12.5 kg bag.',        N'Flour',    N'Bag',     139.50);

INSERT INTO dbo.Products (ProductId, SKU, Name, Description, Category, UnitOfMeasure, BasePrice, IsActive, DiscountTiers)
SELECT NEWID(), p.Sku, p.Name, p.Description, p.Category, p.Unit, p.Price, 1, @tiers
FROM @products AS p
WHERE NOT EXISTS (SELECT 1 FROM dbo.Products AS existing WHERE existing.SKU = p.Sku);

SELECT COUNT(*) AS ProductCount FROM dbo.Products;
