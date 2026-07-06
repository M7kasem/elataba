using ElAtaba.Domain.Entities;
using Elattba.Core.DTOs;

namespace Elattba.Application.Stores;

internal static class StoreMappingExtensions
{
    public static StoreDto ToStoreDto(this Store store) =>
        new(
            store.StoreId,
            store.OwnerId,
            store.Owner?.Email,
            store.ManagerId,
            store.Manager?.Email,
            store.CategoryId,
            store.Category?.Name,
            store.StoreName,
            store.Location,
            store.Description,
            store.Rating,
            store.CreatedAt);
}
