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

            await dbContext.SaveChangesAsync();
        }
    }
}


