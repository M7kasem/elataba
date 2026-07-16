using ElAtaba.Domain.Entities;
using Elattba.Application.Stores;
using Elattba.Core.DTOs;
using System.Threading.Tasks;
using Xunit;

namespace Elattba.Tests
{
    public class StoreQueryServiceTests
    {
        [Fact]
        public async Task GetAllAsync_ReturnsStoresMatchingSearchAndPagination()
        {
            // Arrange
            var uow = new FakeUnitOfWork();
            await uow.Stores.AddAsync(new Store { StoreId = 1, StoreName = "محمصة البن البرازيلي", Description = "أفضل قهوة", Location = "القاهرة" });
            await uow.Stores.AddAsync(new Store { StoreId = 2, StoreName = "التوحيد والنور", Description = "ملابس", Location = "الاسكندرية" });
            await uow.Stores.AddAsync(new Store { StoreId = 3, StoreName = "بن اليمني", Description = "قهوة يمني ممتازة", Location = "الجيزة" });
            
            var storeService = new StoreService(uow, null!, new FakeCurrentUserService());

            var storeParams = new StoreParams
            {
                Search = "بن",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await storeService.GetAllAsync(storeParams);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count); // محمصة البن البرازيلي, بن اليمني
            Assert.Contains(result.Data.Data, s => s.StoreName == "محمصة البن البرازيلي");
            Assert.Contains(result.Data.Data, s => s.StoreName == "بن اليمني");
            Assert.DoesNotContain(result.Data.Data, s => s.StoreName == "التوحيد والنور");
        }

        [Fact]
        public async Task GetAllAsync_ReturnsStoresMatchingLocation()
        {
            // Arrange
            var uow = new FakeUnitOfWork();
            await uow.Stores.AddAsync(new Store { StoreId = 1, StoreName = "فرع 1", Location = "القاهرة" });
            await uow.Stores.AddAsync(new Store { StoreId = 2, StoreName = "فرع 2", Location = "الاسكندرية" });
            
            var storeService = new StoreService(uow, null!, new FakeCurrentUserService());

            var storeParams = new StoreParams
            {
                Location = "القاهرة"
            };

            // Act
            var result = await storeService.GetAllAsync(storeParams);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal(1, result.Data!.Count);
            Assert.Equal("فرع 1", result.Data.Data[0].StoreName);
        }
    }
}
