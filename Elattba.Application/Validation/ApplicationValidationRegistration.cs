using Elattba.Application.Products;
using Elattba.Core.DTOs;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Elattba.Application.Validation;

public static class ApplicationValidationRegistration
{
    public static IServiceCollection AddApplicationValidation(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateOrderDto>, CreateOrderDtoValidator>();
        services.AddScoped<IValidator<UpdateOrderStatusDto>, UpdateOrderStatusDtoValidator>();
        services.AddScoped<IValidator<CreateOrderItemDto>, CreateOrderItemDtoValidator>();
        services.AddScoped<IValidator<UpdateOrderItemDto>, UpdateOrderItemDtoValidator>();
        services.AddScoped<IValidator<CreateCheckoutDto>, CreateCheckoutDtoValidator>();
        services.AddScoped<IValidator<CreateCheckoutItemDto>, CreateCheckoutItemDtoValidator>();

        services.AddScoped<IValidator<CreateOfferDto>, CreateOfferDtoValidator>();
        services.AddScoped<IValidator<UpdateOfferDto>, UpdateOfferDtoValidator>();

        services.AddScoped<IValidator<CreateProductCommand>, CreateProductCommandValidator>();
        services.AddScoped<IValidator<CreateProductWithOfferCommand>, CreateProductWithOfferCommandValidator>();
        services.AddScoped<IValidator<UpdateProductCommand>, UpdateProductCommandValidator>();
        services.AddScoped<IValidator<CreateProductDto>, CreateProductDtoValidator>();
        services.AddScoped<IValidator<UpdateProductDto>, UpdateProductDtoValidator>();

        services.AddScoped<IValidator<CreateReviewDto>, CreateReviewDtoValidator>();
        services.AddScoped<IValidator<UpdateReviewDto>, UpdateReviewDtoValidator>();

        services.AddScoped<IValidator<CreateUserDto>, CreateUserDtoValidator>();
        services.AddScoped<IValidator<UpdateUserDto>, UpdateUserDtoValidator>();

        services.AddScoped<IValidator<CreatePricingTierDto>, CreatePricingTierDtoValidator>();
        services.AddScoped<IValidator<UpdatePricingTierDto>, UpdatePricingTierDtoValidator>();
        services.AddScoped<IValidator<CreateShippingRateDto>, CreateShippingRateDtoValidator>();
        services.AddScoped<IValidator<UpdateShippingRateDto>, UpdateShippingRateDtoValidator>();
        services.AddScoped<IValidator<CreateStoreDto>, CreateStoreDtoValidator>();
        services.AddScoped<IValidator<UpdateStoreDto>, UpdateStoreDtoValidator>();
        services.AddScoped<IValidator<CreateMessageDto>, CreateMessageDtoValidator>();
        services.AddScoped<IValidator<CreateProductImageDto>, CreateProductImageDtoValidator>();
        services.AddScoped<IValidator<UpdateProductImageDto>, UpdateProductImageDtoValidator>();

        return services;
    }
}
