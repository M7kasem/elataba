using ElAtaba.Domain.Entities;
using ElAtaba.Domain.Enums;
using Elattba.Application.Auth;
using Elattba.Core.DTOs;
using Elattba.InfraStructure.Data;
using Elattba.InfraStructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Elattaba.API.Services;

public static class IdentityRoleSeeder
{
    private static readonly string[] Roles =
    [
        AuthConstants.AdminRole,
        AuthConstants.BuyerRole,
        AuthConstants.SellerRole,
        AuthConstants.StoreManagerRole
    ];

    public static async Task SeedIdentityRolesAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<El3atbaDbContext>();

        // 1. Seed Governorates if empty (needed for User registration)
        if (!await dbContext.Governorates.AnyAsync())
        {
            dbContext.Governorates.AddRange(
                new Governorate { Name = "Cairo (القاهرة)" },
                new Governorate { Name = "Giza (الجيزة)" },
                new Governorate { Name = "Alexandria (الإسكندرية)" },
                new Governorate { Name = "Qalyubia (القليوبية)" },
                new Governorate { Name = "Gharbia (الغربية)" }
            );
            await dbContext.SaveChangesAsync();
        }

        // 2. Seed Categories if empty (needed for Store/Product categorization)
        if (!await dbContext.Categories.AnyAsync())
        {
            dbContext.Categories.AddRange(
                new Category { Name = "Fashion & Clothing (الملابس والأزياء)", Description = "ملابس، أحذية، حقائب، وإكسسوارات" },
                new Category { Name = "Electronics & Tech (الإلكترونيات والتقنية)", Description = "مستلزمات هواتف، شواحن، إكسسوارات إلكترونية" },
                new Category { Name = "Home & Kitchen (المنزل والمطبخ)", Description = "أدوات منزلية، أجهزة كهربائية، ديكورات" },
                new Category { Name = "Cosmetics & Beauty (مستحضرات التجميل)", Description = "مكياج، عطور، أدوات العناية الشخصية" }
            );
            await dbContext.SaveChangesAsync();
        }

        // 3. Seed Roles
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 4. Seed Default Users
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var provisioningService = scope.ServiceProvider.GetRequiredService<IUserProvisioningService>();

        await SeedDefaultUserAsync(userManager, provisioningService, "admin@el3ttba.com", "Password123!", "System", "Admin", "01000000001", UserRole.Admin);
        await SeedDefaultUserAsync(userManager, provisioningService, "seller@el3ttba.com", "Password123!", "John", "Seller", "01000000002", UserRole.Seller);
        await SeedDefaultUserAsync(userManager, provisioningService, "seller2@el3ttba.com", "Password123!", "Omar", "Seller 2", "01000000004", UserRole.Seller);
        await SeedDefaultUserAsync(userManager, provisioningService, "buyer@el3ttba.com", "Password123!", "Jane", "Buyer", "01000000003", UserRole.Buyer);

        // 5. Seed Store, Products, and Offers for local demo
        await SeedCatalogAsync(dbContext);
    }

    private static async Task SeedDefaultUserAsync(
        UserManager<AppUser> userManager,
        IUserProvisioningService provisioningService,
        string email,
        string password,
        string firstName,
        string lastName,
        string phone,
        UserRole role)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing == null)
        {
            var registerDto = new RegisterDto(
                email,
                password,
                firstName,
                lastName,
                phone,
                role,
                1, // Cairo
                "Cairo",
                "ElAtaba Market, Cairo"
            );

            await provisioningService.RegisterAsync(registerDto);
        }
    }

    private static async Task SeedCatalogAsync(El3atbaDbContext dbContext)
    {
        var sellerUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "seller@el3ttba.com");
        if (sellerUser == null) return;

        var sellerStore = await dbContext.Stores.FirstOrDefaultAsync(s => s.OwnerId == sellerUser.UserId);
        if (sellerStore == null)
        {
            sellerStore = new Store
            {
                OwnerId = sellerUser.UserId,
                CategoryId = 2, // Electronics
                StoreName = "ElAtaba Tech & Electronics (العتبة للإلكترونيات)",
                Location = "العتبة، شارع الجيش، القاهرة",
                Description = "أكبر تشكيلة من ملحقات الهواتف والالكترونيات بسعر الجملة",
                Rating = 4.8m,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Stores.Add(sellerStore);
            await dbContext.SaveChangesAsync();

            // Add Products
            var prod1 = new Product
            {
                StoreId = sellerStore.StoreId,
                CategoryId = 2,
                Name = "Wireless Headset Max (سماعات رأس لاسلكية)",
                Description = "سماعات رأس لاسلكية عالية الجودة مع خاصية عزل الضوضاء وبطارية تدوم 40 ساعة.",
                BasePrice = 150m,
                StockQuantity = 200,
                HasOffer = true, // We will link an active offer
                CreatedAt = DateTime.UtcNow
            };

            var prod2 = new Product
            {
                StoreId = sellerStore.StoreId,
                CategoryId = 2,
                Name = "Smart Watch Series 9 (ساعة ذكية)",
                Description = "ساعة ذكية تدعم تتبع الأنشطة الرياضية، ضربات القلب، ونظام تحديد المواقع GPS مع شاشة AMOLED.",
                BasePrice = 350m,
                StockQuantity = 100,
                HasOffer = true, // We will link an active offer
                CreatedAt = DateTime.UtcNow
            };

            var prod3 = new Product
            {
                StoreId = sellerStore.StoreId,
                CategoryId = 2,
                Name = "USB-C Fast Charger 20W (شاحن سريع)",
                Description = "شاحن جداري سريع بقدرة 20 وات متوافق مع جميع الهواتف الذكية والأجهزة اللوحية.",
                BasePrice = 45m,
                StockQuantity = 1000,
                HasOffer = false,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Products.AddRange(prod1, prod2, prod3);
            await dbContext.SaveChangesAsync();

            // Add Pricing Tiers
            dbContext.PricingTiers.AddRange(
                new PricingTier { ProductId = prod1.ProductId, MinQuantity = 10, PricePerUnit = 130m },
                new PricingTier { ProductId = prod1.ProductId, MinQuantity = 50, PricePerUnit = 110m },
                new PricingTier { ProductId = prod2.ProductId, MinQuantity = 5, PricePerUnit = 320m },
                new PricingTier { ProductId = prod2.ProductId, MinQuantity = 20, PricePerUnit = 290m },
                new PricingTier { ProductId = prod3.ProductId, MinQuantity = 50, PricePerUnit = 35m },
                new PricingTier { ProductId = prod3.ProductId, MinQuantity = 200, PricePerUnit = 28m }
            );

            // Add Product Images
            dbContext.ProductImages.AddRange(
                new ProductImage { ProductId = prod1.ProductId, ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=500&auto=format&fit=crop", IsPrimary = true, CreatedAt = DateTime.UtcNow },
                new ProductImage { ProductId = prod2.ProductId, ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=500&auto=format&fit=crop", IsPrimary = true, CreatedAt = DateTime.UtcNow },
                new ProductImage { ProductId = prod3.ProductId, ImageUrl = "https://images.unsplash.com/photo-1583863788434-e58a36330cf0?w=500&auto=format&fit=crop", IsPrimary = true, CreatedAt = DateTime.UtcNow }
            );

            // Add an Active Offer (15% off on the first two products)
            var offer = new Offer
            {
                StoreId = sellerStore.StoreId,
                DiscountPercentage = 15.00m,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(7),
                AppliesToAllProducts = false,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Offers.Add(offer);
            await dbContext.SaveChangesAsync();

            dbContext.OfferProducts.AddRange(
                new OfferProduct { OfferId = offer.OfferId, ProductId = prod1.ProductId },
                new OfferProduct { OfferId = offer.OfferId, ProductId = prod2.ProductId }
            );

            // Ensure at least 10 products total (Adding more to first store)
            var prod4 = new Product { StoreId = sellerStore.StoreId, CategoryId = 2, Name = "Power Bank 20000mAh", Description = "شاحن متنقل عالي السعة", BasePrice = 250m, StockQuantity = 50, HasOffer = false, CreatedAt = DateTime.UtcNow };
            var prod5 = new Product { StoreId = sellerStore.StoreId, CategoryId = 2, Name = "Laptop Stand Aluminum", Description = "حامل لاب توب معدني قابل للتعديل", BasePrice = 120m, StockQuantity = 75, HasOffer = false, CreatedAt = DateTime.UtcNow };
            var prod6 = new Product { StoreId = sellerStore.StoreId, CategoryId = 2, Name = "Wireless Mouse", Description = "ماوس لاسلكي صامت", BasePrice = 85m, StockQuantity = 300, HasOffer = false, CreatedAt = DateTime.UtcNow };

            dbContext.Products.AddRange(prod4, prod5, prod6);
            await dbContext.SaveChangesAsync();
            
            // Add Pricing Tiers and Images for them
            dbContext.PricingTiers.AddRange(
                new PricingTier { ProductId = prod4.ProductId, MinQuantity = 10, PricePerUnit = 220m },
                new PricingTier { ProductId = prod5.ProductId, MinQuantity = 20, PricePerUnit = 100m },
                new PricingTier { ProductId = prod6.ProductId, MinQuantity = 50, PricePerUnit = 65m }
            );

            dbContext.ProductImages.AddRange(
                new ProductImage { ProductId = prod4.ProductId, ImageUrl = "https://images.unsplash.com/photo-1609091839311-d5365f9ff1c5?w=500&auto=format&fit=crop", IsPrimary = true, CreatedAt = DateTime.UtcNow },
                new ProductImage { ProductId = prod5.ProductId, ImageUrl = "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=500&auto=format&fit=crop", IsPrimary = true, CreatedAt = DateTime.UtcNow },
                new ProductImage { ProductId = prod6.ProductId, ImageUrl = "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=500&auto=format&fit=crop", IsPrimary = true, CreatedAt = DateTime.UtcNow }
            );

            await dbContext.SaveChangesAsync();
        }

        // Add Store 2
        var sellerUser2 = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "seller2@el3ttba.com");
        if (sellerUser2 != null)
        {
            var sellerStore2 = await dbContext.Stores.FirstOrDefaultAsync(s => s.OwnerId == sellerUser2.UserId);
            if (sellerStore2 == null)
            {
                sellerStore2 = new Store
                {
                    OwnerId = sellerUser2.UserId,
                    CategoryId = 1, // Fashion
                    StoreName = "ElAtaba Fashion Hub (العتبة للأزياء)",
                    Location = "العتبة، شارع عبد العزيز، القاهرة",
                    Description = "أحدث الموديلات الكاجوال والكلاسيك",
                    Rating = 4.5m,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.Stores.Add(sellerStore2);
                await dbContext.SaveChangesAsync();

                // Add Products for Store 2
                var prod7 = new Product { StoreId = sellerStore2.StoreId, CategoryId = 1, Name = "Men's Classic Shirt", Description = "قميص رجالي كلاسيك قطن 100%", BasePrice = 200m, StockQuantity = 150, HasOffer = true, CreatedAt = DateTime.UtcNow };
                var prod8 = new Product { StoreId = sellerStore2.StoreId, CategoryId = 1, Name = "Women's Summer Dress", Description = "فستان صيفي نسائي بألوان زاهية", BasePrice = 300m, StockQuantity = 80, HasOffer = false, CreatedAt = DateTime.UtcNow };
                var prod9 = new Product { StoreId = sellerStore2.StoreId, CategoryId = 1, Name = "Denim Jeans", Description = "بنطلون جينز رجالي جودة عالية", BasePrice = 250m, StockQuantity = 200, HasOffer = false, CreatedAt = DateTime.UtcNow };
                var prod10 = new Product { StoreId = sellerStore2.StoreId, CategoryId = 1, Name = "Leather Wallet", Description = "محفظة جلد طبيعي", BasePrice = 150m, StockQuantity = 500, HasOffer = true, CreatedAt = DateTime.UtcNow };

                dbContext.Products.AddRange(prod7, prod8, prod9, prod10);
                await dbContext.SaveChangesAsync();

                dbContext.PricingTiers.AddRange(
                    new PricingTier { ProductId = prod7.ProductId, MinQuantity = 12, PricePerUnit = 170m },
                    new PricingTier { ProductId = prod8.ProductId, MinQuantity = 10, PricePerUnit = 250m },
                    new PricingTier { ProductId = prod9.ProductId, MinQuantity = 20, PricePerUnit = 200m },
                    new PricingTier { ProductId = prod10.ProductId, MinQuantity = 50, PricePerUnit = 100m }
                );

                dbContext.ProductImages.AddRange(
                    new ProductImage { ProductId = prod7.ProductId, ImageUrl = "https://images.unsplash.com/photo-1596755094514-f87e32f85e2c?w=500&auto=format&fit=crop", IsPrimary = true, CreatedAt = DateTime.UtcNow },
                    new ProductImage { ProductId = prod8.ProductId, ImageUrl = "https://images.unsplash.com/photo-1572804013309-59a88b7e92f1?w=500&auto=format&fit=crop", IsPrimary = true, CreatedAt = DateTime.UtcNow },
                    new ProductImage { ProductId = prod9.ProductId, ImageUrl = "https://images.unsplash.com/photo-1542272604-787c3835535d?w=500&auto=format&fit=crop", IsPrimary = true, CreatedAt = DateTime.UtcNow },
                    new ProductImage { ProductId = prod10.ProductId, ImageUrl = "https://images.unsplash.com/photo-1627123424574-724758594e93?w=500&auto=format&fit=crop", IsPrimary = true, CreatedAt = DateTime.UtcNow }
                );

                var offer2 = new Offer
                {
                    StoreId = sellerStore2.StoreId,
                    DiscountPercentage = 20.00m,
                    StartDate = DateTime.UtcNow.AddDays(-2),
                    EndDate = DateTime.UtcNow.AddDays(5),
                    AppliesToAllProducts = false,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.Offers.Add(offer2);
                await dbContext.SaveChangesAsync();

                dbContext.OfferProducts.AddRange(
                    new OfferProduct { OfferId = offer2.OfferId, ProductId = prod7.ProductId },
                    new OfferProduct { OfferId = offer2.OfferId, ProductId = prod10.ProductId }
                );

                await dbContext.SaveChangesAsync();
            }
        }

        // Add Carriers and Shipping Rates
        if (!await dbContext.Carriers.AnyAsync())
        {
            var carrier1 = new Carrier { Name = "Aramex (أرامكس)", IsActive = true };
            var carrier2 = new Carrier { Name = "Bosta (بوسطة)", IsActive = true };
            dbContext.Carriers.AddRange(carrier1, carrier2);
            await dbContext.SaveChangesAsync();

            dbContext.ShippingRates.AddRange(
                new ShippingRate { CarrierId = carrier1.CarrierId, GovernorateId = 1 /* Cairo */, Cost = 50m },
                new ShippingRate { CarrierId = carrier1.CarrierId, GovernorateId = 2 /* Giza */, Cost = 60m },
                new ShippingRate { CarrierId = carrier2.CarrierId, GovernorateId = 1 /* Cairo */, Cost = 45m },
                new ShippingRate { CarrierId = carrier2.CarrierId, GovernorateId = 3 /* Alex */, Cost = 80m }
            );
            await dbContext.SaveChangesAsync();
        }

        // Add Out of Stock Product for testing
        var adminStore = await dbContext.Stores.FirstOrDefaultAsync(s => s.StoreName == "ElAtaba Tech & Electronics (العتبة للإلكترونيات)");
        if (adminStore != null && !await dbContext.Products.AnyAsync(p => p.Name == "Sold Out Item"))
        {
            var outOfStockProd = new Product 
            { 
                StoreId = adminStore.StoreId, 
                CategoryId = 2, 
                Name = "Sold Out Item", 
                Description = "This item is out of stock for testing", 
                BasePrice = 99m, 
                StockQuantity = 0, 
                HasOffer = false, 
                CreatedAt = DateTime.UtcNow 
            };
            dbContext.Products.Add(outOfStockProd);
            await dbContext.SaveChangesAsync();
        }

        // Add Orders
        if (!await dbContext.Orders.AnyAsync())
        {
            var buyerUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "buyer@el3ttba.com");
            var store1 = await dbContext.Stores.FirstOrDefaultAsync(s => s.StoreName == "ElAtaba Tech & Electronics (العتبة للإلكترونيات)");
            var store2 = await dbContext.Stores.FirstOrDefaultAsync(s => s.StoreName == "ElAtaba Fashion Hub (العتبة للأزياء)");
            var carrier = await dbContext.Carriers.FirstOrDefaultAsync();

            if (buyerUser != null && store1 != null && store2 != null && carrier != null)
            {
                var prod1 = await dbContext.Products.FirstOrDefaultAsync(p => p.StoreId == store1.StoreId);
                var prod2 = await dbContext.Products.FirstOrDefaultAsync(p => p.StoreId == store2.StoreId);

                if (prod1 != null && prod2 != null)
                {
                    // Order 1: Delivered
                    var order1 = new Order
                    {
                        BuyerId = buyerUser.UserId,
                        StoreId = store1.StoreId,
                        CarrierId = carrier.CarrierId,
                        Status = OrderStatus.Delivered,
                        PaymentMethod = PaymentMethod.Cash,
                        PaymentStatus = PaymentStatus.Paid,
                        ShippingAddressSnapshot = "123 Test St, Cairo",
                        OrderDate = DateTime.UtcNow.AddDays(-10),
                        TotalAmount = 550m,
                        ShippingCost = 50m,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Order 2: Pending
                    var order2 = new Order
                    {
                        BuyerId = buyerUser.UserId,
                        StoreId = store2.StoreId,
                        CarrierId = carrier.CarrierId,
                        Status = OrderStatus.Pending,
                        PaymentMethod = PaymentMethod.Online,
                        PaymentStatus = PaymentStatus.Paid,
                        ShippingAddressSnapshot = "123 Test St, Cairo",
                        OrderDate = DateTime.UtcNow,
                        TotalAmount = 250m,
                        ShippingCost = 45m,
                        CreatedAt = DateTime.UtcNow
                    };

                    dbContext.Orders.AddRange(order1, order2);
                    await dbContext.SaveChangesAsync();

                    dbContext.OrderItems.AddRange(
                        new OrderItem { OrderId = order1.OrderId, ProductId = prod1.ProductId, Quantity = 5, UnitPrice = 100m, Subtotal = 500m },
                        new OrderItem { OrderId = order2.OrderId, ProductId = prod2.ProductId, Quantity = 1, UnitPrice = 205m, Subtotal = 205m }
                    );

                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}


