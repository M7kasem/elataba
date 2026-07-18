using ElAtaba.Domain.Entities;
using ElAtaba.Domain.Enums;
using Elattba.Application.Auth;
using Elattba.Core.DTOs;
using Elattba.InfraStructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Elattaba.API.Services;

/// <summary>
/// Development-friendly marketplace data. Every operation is idempotent, so the
/// API can be restarted without duplicating demo records.
/// Product imagery intentionally contains products only—no people.
/// </summary>
internal static class DemoCatalogSeeder
{
    private const string DemoPassword = "Password123!";

    public static async Task SeedAsync(
        El3atbaDbContext dbContext,
        IUserProvisioningService provisioningService)
    {
        await EnsureCategoriesAsync(dbContext);
        await EnsureUsersAsync(dbContext, provisioningService);
        await ReplaceLegacyFashionImageAsync(dbContext);

        var homeStore = await SeedStoreAsync(
            dbContext,
            "seller3@el3ttba.com",
            "Nile Home & Kitchen",
            "Downtown Cairo",
            "Wholesale cookware, storage, lighting, and everyday home essentials.",
            3,
            4.7m,
            [
                new("Non-Stick Cookware Set", "Seven-piece granite cookware set for busy kitchens.", 1850m, 42, "photo-1556911220-bff31c812dba"),
                new("Bamboo Storage Basket", "Durable woven organizer for shelves, bedrooms, and kitchens.", 260m, 180, "photo-1594223274512-ad4803739b7c"),
                new("Stainless Steel Thermos 1L", "Leak-resistant insulated bottle that keeps drinks hot or cold.", 310m, 220, "photo-1544003484-3cd181d17917"),
                new("LED Desk Lamp", "Adjustable LED lamp with three brightness levels and USB power.", 490m, 95, "photo-1534073828943-f801091bb18c"),
                new("Microfiber Cleaning Set", "Reusable cleaning cloths and scrubbers for home and office.", 145m, 360, "photo-1583947215259-38e31be8751f"),
                new("Electric Kettle 1.7L", "Fast-boil stainless steel kettle with automatic shutoff.", 780m, 64, "photo-1544986581-efac024faf62")
            ],
            12m);

        var hardwareStore = await SeedStoreAsync(
            dbContext,
            "seller4@el3ttba.com",
            "Cairo Tools & Hardware",
            "El Galaa Street, Cairo",
            "Reliable hand tools, workshop accessories, and maintenance supplies.",
            5,
            4.6m,
            [
                new("Cordless Drill Set", "18V drill with two batteries, bit set, and carrying case.", 2450m, 38, "photo-1504148455328-c376907d081c"),
                new("Professional Screwdriver Kit", "Magnetic precision screwdriver set with 32 interchangeable bits.", 420m, 150, "photo-1586864387789-628af9feed72"),
                new("Heavy Duty Extension Cable", "Ten-meter insulated extension cable with grounded sockets.", 360m, 130, "photo-1558618666-fcd25c85cd64"),
                new("Digital Measuring Tape", "Five-meter measuring tape with clear digital display.", 275m, 115, "photo-1503387762-592deb58ef4e"),
                new("Safety Work Gloves", "Grip-enhanced protective gloves for workshop and warehouse use.", 125m, 500, "photo-1586864387967-d02ef85d93e8"),
                new("Adjustable Wrench 12 Inch", "Chrome vanadium wrench for plumbing and general repairs.", 290m, 190, "photo-1581147036324-c1c4c3b0c9f6")
            ],
            10m);

        var mensStore = await SeedStoreAsync(
            dbContext,
            "seller5@el3ttba.com",
            "Al-Madina Men's Essentials",
            "Muski Market, Cairo",
            "Men's clothing, shoes, bags, and practical accessories at wholesale prices.",
            1,
            4.5m,
            [
                new("Men's Oxford Shirt", "Classic long-sleeve cotton shirt for work and formal occasions.", 420m, 140, "photo-1596755094514-f87e32f85e2c"),
                new("Men's Polo Shirt", "Soft pique polo shirt with a structured collar and durable finish.", 320m, 210, "photo-1625910513413-5fc1c8d6b5f4"),
                new("Canvas Sneakers", "Everyday low-top sneakers with a non-slip rubber sole.", 560m, 120, "photo-1542291026-7eec264c27ff"),
                new("Slim Leather Belt", "Genuine leather belt with a brushed-metal buckle.", 230m, 270, "photo-1624222247344-550fb60583dc"),
                new("Travel Duffel Bag", "Water-resistant carry-on bag with separate shoe compartment.", 690m, 85, "photo-1553062407-98eeb64c6a62"),
                new("Classic Analog Watch", "Stainless steel watch with a clean, easy-to-read dial.", 980m, 70, "photo-1524805444758-089113d48a6d")
            ],
            15m);

        var pantryStore = await SeedStoreAsync(
            dbContext,
            "seller6@el3ttba.com",
            "Market Pantry Wholesale",
            "Ataba Square, Cairo",
            "Bulk coffee, pantry staples, snacks, and packaging for cafés and retailers.",
            6,
            4.8m,
            [
                new("Arabica Coffee Beans 1kg", "Freshly roasted medium blend for espresso and filter coffee.", 620m, 160, "photo-1495474472287-4d71bcdd2085"),
                new("Premium Tea Collection", "Assorted black, green, and herbal tea bags in a gift box.", 285m, 240, "photo-1544787219-7f47ccb76574"),
                new("Raw Egyptian Honey 500g", "Natural floral honey in a sealed glass jar.", 240m, 190, "photo-1587049352846-4a222e784d38"),
                new("Roasted Nuts Mix 500g", "Salted almonds, cashews, and peanuts for shops and cafés.", 355m, 175, "photo-1599599810694-b5b37304c041"),
                new("Kraft Food Packaging Set", "Eco-friendly takeaway boxes and paper bags, pack of 100.", 430m, 110, "photo-1524758631624-e2822e304c36"),
                new("Sparkling Water Case", "Twenty-four 330ml cans for offices, cafés, and events.", 215m, 300, "photo-1548839140-29a749e1cf4d")
            ],
            8m);

        await EnsureExpandedShippingAsync(dbContext);
        await SeedOrdersReviewsAndMessagesAsync(dbContext, [homeStore, hardwareStore, mensStore, pantryStore]);
    }

    private static async Task EnsureCategoriesAsync(El3atbaDbContext dbContext)
    {
        var categories = new[]
        {
            new Category { Name = "Tools & Hardware", Description = "Hand tools, workshop supplies, electrical accessories, and maintenance essentials." },
            new Category { Name = "Grocery & Pantry", Description = "Coffee, tea, snacks, food packaging, and wholesale pantry supplies." },
            new Category { Name = "Office & Stationery", Description = "Paper goods, desk supplies, organization, and office accessories." }
        };

        foreach (var category in categories)
        {
            if (!await dbContext.Categories.AnyAsync(item => item.Name == category.Name))
            {
                dbContext.Categories.Add(category);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task EnsureUsersAsync(
        El3atbaDbContext dbContext,
        IUserProvisioningService provisioningService)
    {
        var users = new[]
        {
            new DemoUser("seller3@el3ttba.com", "Khaled", "Home", "01000000005", UserRole.Seller, 1, "Cairo"),
            new DemoUser("seller4@el3ttba.com", "Mahmoud", "Tools", "01000000006", UserRole.Seller, 2, "Giza"),
            new DemoUser("seller5@el3ttba.com", "Youssef", "Menswear", "01000000007", UserRole.Seller, 1, "Cairo"),
            new DemoUser("seller6@el3ttba.com", "Karim", "Pantry", "01000000008", UserRole.Seller, 3, "Alexandria"),
            new DemoUser("buyer2@el3ttba.com", "Ahmed", "Hassan", "01000000009", UserRole.Buyer, 2, "Giza"),
            new DemoUser("buyer3@el3ttba.com", "Mostafa", "Ali", "01000000010", UserRole.Buyer, 3, "Alexandria"),
            new DemoUser("buyer4@el3ttba.com", "Tarek", "Samir", "01000000011", UserRole.Buyer, 1, "Cairo")
        };

        foreach (var user in users)
        {
            if (await dbContext.Users.AnyAsync(item => item.Email == user.Email))
            {
                continue;
            }

            await provisioningService.RegisterAsync(new RegisterDto(
                user.Email,
                DemoPassword,
                user.FirstName,
                user.LastName,
                user.Phone,
                user.Role,
                user.GovernorateId,
                user.City,
                $"Demo wholesale address, {user.City}"));
        }
    }

    private static async Task ReplaceLegacyFashionImageAsync(El3atbaDbContext dbContext)
    {
        var legacyProduct = await dbContext.Products
            .Include(product => product.Images)
            .FirstOrDefaultAsync(product => product.Name == "Women's Summer Dress");

        if (legacyProduct == null)
        {
            return;
        }

        legacyProduct.Name = "Unisex Oversized Hoodie";
        legacyProduct.Description = "Heavyweight cotton hoodie in neutral colors for everyday wholesale orders.";
        legacyProduct.BasePrice = 360m;
        dbContext.ProductImages.RemoveRange(legacyProduct.Images);
        legacyProduct.Images.Add(new ProductImage
        {
            ImageUrl = ImageUrl("photo-1576566588028-4147f3842f27"),
            IsPrimary = true,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }

    private static async Task<Store> SeedStoreAsync(
        El3atbaDbContext dbContext,
        string ownerEmail,
        string storeName,
        string location,
        string description,
        int categoryId,
        decimal rating,
        IReadOnlyList<DemoProduct> products,
        decimal discountPercentage)
    {
        var owner = await dbContext.Users.SingleAsync(user => user.Email == ownerEmail);
        var store = await dbContext.Stores.FirstOrDefaultAsync(item => item.OwnerId == owner.UserId);

        if (store == null)
        {
            store = new Store
            {
                OwnerId = owner.UserId,
                CategoryId = categoryId,
                StoreName = storeName,
                Location = location,
                Description = description,
                Rating = rating,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.Stores.Add(store);
            await dbContext.SaveChangesAsync();
        }

        foreach (var seed in products)
        {
            var product = await dbContext.Products
                .Include(item => item.Images)
                .Include(item => item.PricingTiers)
                .FirstOrDefaultAsync(item => item.StoreId == store.StoreId && item.Name == seed.Name);

            if (product == null)
            {
                product = new Product
                {
                    StoreId = store.StoreId,
                    CategoryId = categoryId,
                    Name = seed.Name,
                    Description = seed.Description,
                    BasePrice = seed.Price,
                    StockQuantity = seed.Stock,
                    HasOffer = true,
                    CreatedAt = DateTime.UtcNow
                };
                dbContext.Products.Add(product);
                await dbContext.SaveChangesAsync();
            }

            if (product.Images.Count == 0)
            {
                dbContext.ProductImages.Add(new ProductImage
                {
                    ProductId = product.ProductId,
                    ImageUrl = ImageUrl(seed.ImageId),
                    IsPrimary = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (product.PricingTiers.Count == 0)
            {
                dbContext.PricingTiers.AddRange(
                    new PricingTier { ProductId = product.ProductId, MinQuantity = 10, PricePerUnit = decimal.Round(seed.Price * 0.92m, 2) },
                    new PricingTier { ProductId = product.ProductId, MinQuantity = 40, PricePerUnit = decimal.Round(seed.Price * 0.85m, 2) });
            }
        }

        if (!await dbContext.Offers.AnyAsync(item => item.StoreId == store.StoreId && item.AppliesToAllProducts))
        {
            dbContext.Offers.Add(new Offer
            {
                StoreId = store.StoreId,
                DiscountPercentage = discountPercentage,
                StartDate = DateTime.UtcNow.AddDays(-2),
                EndDate = DateTime.UtcNow.AddDays(30),
                AppliesToAllProducts = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync();
        return store;
    }

    private static async Task EnsureExpandedShippingAsync(El3atbaDbContext dbContext)
    {
        var carriers = new[]
        {
            new Carrier { Name = "Mile Express", IsActive = true },
            new Carrier { Name = "QuickShip Egypt", IsActive = true }
        };

        foreach (var carrier in carriers)
        {
            if (!await dbContext.Carriers.AnyAsync(item => item.Name == carrier.Name))
            {
                dbContext.Carriers.Add(carrier);
            }
        }
        await dbContext.SaveChangesAsync();

        var carrierList = await dbContext.Carriers.ToListAsync();
        var governorateList = await dbContext.Governorates.ToListAsync();
        foreach (var carrier in carrierList.Where(item => item.IsActive))
        {
            foreach (var governorate in governorateList)
            {
                if (await dbContext.ShippingRates.AnyAsync(rate =>
                    rate.CarrierId == carrier.CarrierId && rate.GovernorateId == governorate.GovernorateId))
                {
                    continue;
                }

                dbContext.ShippingRates.Add(new ShippingRate
                {
                    CarrierId = carrier.CarrierId,
                    GovernorateId = governorate.GovernorateId,
                    Cost = 45m + (governorate.GovernorateId * 10m)
                });
            }
        }
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedOrdersReviewsAndMessagesAsync(
        El3atbaDbContext dbContext,
        IReadOnlyList<Store> stores)
    {
        var buyers = await dbContext.Users
            .Where(user => user.Email == "buyer@el3ttba.com" || user.Email == "buyer2@el3ttba.com" || user.Email == "buyer3@el3ttba.com" || user.Email == "buyer4@el3ttba.com")
            .ToDictionaryAsync(user => user.Email);
        var carrier = await dbContext.Carriers.OrderBy(item => item.CarrierId).FirstAsync();

        var orders = new[]
        {
            new DemoOrder("DEMO-1001", "buyer2@el3ttba.com", stores[0], "Non-Stick Cookware Set", 2, OrderStatus.Delivered, PaymentMethod.Cash, PaymentStatus.Paid, 5, "Great quality and quick delivery."),
            new DemoOrder("DEMO-1002", "buyer3@el3ttba.com", stores[0], "LED Desk Lamp", 4, OrderStatus.Shipped, PaymentMethod.Online, PaymentStatus.Paid, 3, null),
            new DemoOrder("DEMO-1003", "buyer4@el3ttba.com", stores[1], "Cordless Drill Set", 1, OrderStatus.Confirmed, PaymentMethod.Cash, PaymentStatus.Pending, 2, null),
            new DemoOrder("DEMO-1004", "buyer2@el3ttba.com", stores[1], "Professional Screwdriver Kit", 8, OrderStatus.Delivered, PaymentMethod.Cash, PaymentStatus.Paid, 9, "Exactly what our workshop needed."),
            new DemoOrder("DEMO-1005", "buyer3@el3ttba.com", stores[2], "Canvas Sneakers", 6, OrderStatus.Pending, PaymentMethod.Online, PaymentStatus.Pending, 1, null),
            new DemoOrder("DEMO-1006", "buyer4@el3ttba.com", stores[2], "Travel Duffel Bag", 3, OrderStatus.Delivered, PaymentMethod.Cash, PaymentStatus.Paid, 14, "Strong material and useful compartments."),
            new DemoOrder("DEMO-1007", "buyer2@el3ttba.com", stores[3], "Arabica Coffee Beans 1kg", 12, OrderStatus.Shipped, PaymentMethod.Cash, PaymentStatus.Paid, 4, null),
            new DemoOrder("DEMO-1008", "buyer3@el3ttba.com", stores[3], "Kraft Food Packaging Set", 5, OrderStatus.Cancelled, PaymentMethod.Online, PaymentStatus.Failed, 18, null)
        };

        foreach (var seed in orders)
        {
            if (await dbContext.Orders.AnyAsync(order => order.TrackingNumber == seed.TrackingNumber))
            {
                continue;
            }

            var product = await dbContext.Products.SingleAsync(item => item.StoreId == seed.Store.StoreId && item.Name == seed.ProductName);
            var unitPrice = decimal.Round(product.BasePrice * 0.9m, 2);
            var order = new Order
            {
                BuyerId = buyers[seed.BuyerEmail].UserId,
                StoreId = seed.Store.StoreId,
                CarrierId = carrier.CarrierId,
                TrackingNumber = seed.TrackingNumber,
                Status = seed.Status,
                PaymentMethod = seed.PaymentMethod,
                PaymentStatus = seed.PaymentStatus,
                ShippingAddressSnapshot = $"Demo customer address, {buyers[seed.BuyerEmail].City}",
                OrderDate = DateTime.UtcNow.AddDays(-seed.DaysAgo),
                TotalAmount = unitPrice * seed.Quantity,
                ShippingCost = 55m,
                CreatedAt = DateTime.UtcNow.AddDays(-seed.DaysAgo)
            };
            dbContext.Orders.Add(order);
            await dbContext.SaveChangesAsync();

            dbContext.OrderItems.Add(new OrderItem
            {
                OrderId = order.OrderId,
                ProductId = product.ProductId,
                Quantity = seed.Quantity,
                UnitPrice = unitPrice,
                Subtotal = unitPrice * seed.Quantity
            });

            if (seed.ReviewComment != null)
            {
                dbContext.Reviews.Add(new Review
                {
                    OrderId = order.OrderId,
                    StoreId = seed.Store.StoreId,
                    BuyerId = buyers[seed.BuyerEmail].UserId,
                    Rating = 5,
                    Comment = seed.ReviewComment,
                    CreatedAt = DateTime.UtcNow.AddDays(-Math.Max(1, seed.DaysAgo - 1))
                });
            }
            await dbContext.SaveChangesAsync();
        }

        var messageSeeds = new[]
        {
            new DemoMessage("buyer2@el3ttba.com", stores[0], "Non-Stick Cookware Set", "Can you prepare a carton price for 20 sets?"),
            new DemoMessage("buyer3@el3ttba.com", stores[1], "Cordless Drill Set", "Is an invoice included with the order?"),
            new DemoMessage("buyer4@el3ttba.com", stores[2], "Travel Duffel Bag", "Please confirm the available colors before shipping."),
            new DemoMessage("buyer2@el3ttba.com", stores[3], "Arabica Coffee Beans 1kg", "Do you have a recurring weekly supply option?")
        };

        foreach (var seed in messageSeeds)
        {
            if (await dbContext.Messages.AnyAsync(message => message.MessageText == seed.Text))
            {
                continue;
            }

            var product = await dbContext.Products.SingleAsync(item => item.StoreId == seed.Store.StoreId && item.Name == seed.ProductName);
            dbContext.Messages.Add(new Message
            {
                SenderId = buyers[seed.BuyerEmail].UserId,
                RecipientId = seed.Store.OwnerId,
                ProductId = product.ProductId,
                MessageText = seed.Text,
                SentAt = DateTime.UtcNow.AddHours(-2),
                IsRead = false
            });
        }
        await dbContext.SaveChangesAsync();
    }

    private static string ImageUrl(string imageId) =>
        $"https://images.unsplash.com/{imageId}?auto=format&fit=crop&w=900&q=80";

    private sealed record DemoUser(
        string Email,
        string FirstName,
        string LastName,
        string Phone,
        UserRole Role,
        int GovernorateId,
        string City);

    private sealed record DemoProduct(string Name, string Description, decimal Price, int Stock, string ImageId);

    private sealed record DemoOrder(
        string TrackingNumber,
        string BuyerEmail,
        Store Store,
        string ProductName,
        int Quantity,
        OrderStatus Status,
        PaymentMethod PaymentMethod,
        PaymentStatus PaymentStatus,
        int DaysAgo,
        string? ReviewComment);

    private sealed record DemoMessage(string BuyerEmail, Store Store, string ProductName, string Text);
}
