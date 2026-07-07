using Elattaba.API.BackgroundServices;
using Elattaba.API.ExceptionHandling;
using Elattaba.API.Controllers;
using Elattaba.API.Services;
using Elattaba.API.Validation;
using Elattba.Application.Carriers;
using Elattba.Application.Categories;
using Elattba.Application.Governorates;
using Elattba.Application.Messages;
using Elattba.Application.Offers;
using Elattba.Application.Orders;
using Elattba.Application.PricingTiers;
using Elattba.Application.ProductImages;
using Elattba.Application.Products;
using Elattba.Application.Reviews;
using Elattba.Application.ShippingRates;
using Elattba.Application.Stores;
using Elattba.Application.Users;
using Elattba.Application.Validation;
using Elattba.InfraStructure;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Extensions
{
    public static class ApiServiceExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers(options =>
            {
                options.Filters.Add<FluentValidationFilter>();
            });
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "One or more validation errors occurred."
                    };
                    problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                    return new BadRequestObjectResult(problemDetails);
                };
            });
            services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                };
            });
            services.AddExceptionHandler<ConcurrencyExceptionHandler>();
            services.AddExceptionHandler<DatabaseExceptionHandler>();
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.InfrastructureConfiguration(configuration);
            services.AddScoped<ICarrierService, CarrierService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IGovernorateService, GovernorateService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderItemService, OrderItemService>();
            services.AddScoped<IOfferService, OfferService>();
            services.AddScoped<IPricingTierService, PricingTierService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IProductImageService, ProductImageService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IShippingRateService, ShippingRateService>();
            services.AddScoped<IStoreService, StoreService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPasswordHashingService, AspNetCorePasswordHashingService>();
            services.AddSingleton<IProductImageEmbeddingQueue, ChannelProductImageEmbeddingQueue>();
            services.AddHostedService<ProductImageEmbeddingBackgroundService>();
            services.AddApplicationValidation();
            services.AddScoped<IValidator<ProductController.CreateProductFormDto>, CreateProductFormDtoValidator>();
            services.AddScoped<IValidator<ProductController.CreateProductWithOfferFormDto>, CreateProductWithOfferFormDtoValidator>();
            services.AddScoped<IValidator<ProductController.UpdateProductFormDto>, UpdateProductFormDtoValidator>();
            services.AddScoped<IValidator<ProductImageController.UploadProductImageFormDto>, UploadProductImageFormDtoValidator>();
            services.AddScoped<IValidator<ProductImageController.UploadManyProductImagesFormDto>, UploadManyProductImagesFormDtoValidator>();
            services.AddApiRateLimiting();

            return services;
        }
    }
}
