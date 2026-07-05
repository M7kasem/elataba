using ElAtaba.Domain.Entities;
using Elattaba.API.Helper;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OfferController : BaseController
{
    public OfferController(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var offers = await _unitOfWork.Offers.GetAllAsync(
                offer => offer.Store!,
                offer => offer.OfferProducts);
            var data = offers.Select(offer => offer.ToDto()).ToList();
            return Ok(new ResponseAPI(200, "Offers retrieved successfully", data));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var offers = await _unitOfWork.Offers.GetAllAsync(
                offer => offer.Store!,
                offer => offer.OfferProducts);
            var offer = offers.FirstOrDefault(item => item.OfferId == id);
            if (offer == null)
            {
                return NotFound(new ResponseAPI(404, "Offer not found."));
            }

            return Ok(new ResponseAPI(200, "Offer retrieved successfully", offer.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOfferDto createOfferDto)
    {
        try
        {
            var productIds = createOfferDto.ProductIds.Distinct().ToList();
            var validationError = ValidateOffer(
                createOfferDto.DiscountPercentage,
                createOfferDto.StartDate,
                createOfferDto.EndDate,
                createOfferDto.AppliesToAllProducts,
                productIds);
            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            var store = await _unitOfWork.Stores.GetByIdAsync(createOfferDto.StoreId);
            if (store == null)
            {
                return NotFound(new ResponseAPI(404, "Store not found."));
            }

            var offer = new Offer
            {
                StoreId = createOfferDto.StoreId,
                DiscountPercentage = createOfferDto.DiscountPercentage,
                StartDate = createOfferDto.StartDate,
                EndDate = createOfferDto.EndDate,
                AppliesToAllProducts = createOfferDto.AppliesToAllProducts
            };

            foreach (var productId in productIds)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product == null)
                {
                    return NotFound(new ResponseAPI(404, $"Product {productId} not found."));
                }

                if (product.StoreId != createOfferDto.StoreId)
                {
                    return BadRequest(new ResponseAPI(400, $"Product {productId} does not belong to store {createOfferDto.StoreId}."));
                }

                offer.OfferProducts.Add(new OfferProduct
                {
                    ProductId = productId
                });
            }

            await _unitOfWork.Offers.AddAsync(offer);
            await _unitOfWork.CompleteAsync();

            offer.Store = store;
            return CreatedAtAction(
                nameof(GetById),
                new { id = offer.OfferId },
                new ResponseAPI(201, "Offer created successfully", offer.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOfferDto updateOfferDto)
    {
        try
        {
            var offers = await _unitOfWork.Offers.GetAllAsync(
                offer => offer.Store!,
                offer => offer.OfferProducts);
            var offer = offers.FirstOrDefault(item => item.OfferId == id);
            if (offer == null)
            {
                return NotFound(new ResponseAPI(404, "Offer not found."));
            }

            var productIds = updateOfferDto.ProductIds.Distinct().ToList();
            var validationError = ValidateOffer(
                updateOfferDto.DiscountPercentage,
                updateOfferDto.StartDate,
                updateOfferDto.EndDate,
                updateOfferDto.AppliesToAllProducts,
                productIds);
            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            offer.DiscountPercentage = updateOfferDto.DiscountPercentage;
            offer.StartDate = updateOfferDto.StartDate;
            offer.EndDate = updateOfferDto.EndDate;
            offer.AppliesToAllProducts = updateOfferDto.AppliesToAllProducts;
            offer.OfferProducts.Clear();

            foreach (var productId in productIds)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product == null)
                {
                    return NotFound(new ResponseAPI(404, $"Product {productId} not found."));
                }

                if (product.StoreId != offer.StoreId)
                {
                    return BadRequest(new ResponseAPI(400, $"Product {productId} does not belong to store {offer.StoreId}."));
                }

                offer.OfferProducts.Add(new OfferProduct
                {
                    OfferId = offer.OfferId,
                    ProductId = productId
                });
            }

            await _unitOfWork.Offers.UpdateAsync(offer);
            await _unitOfWork.CompleteAsync();

            return Ok(new ResponseAPI(200, "Offer updated successfully", offer.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var offer = await _unitOfWork.Offers.GetByIdAsync(id);
            if (offer == null)
            {
                return NotFound(new ResponseAPI(404, "Offer not found."));
            }

            await _unitOfWork.Offers.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return Ok(new ResponseAPI(200, "Offer deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    private static ResponseAPI? ValidateOffer(
        decimal discountPercentage,
        DateTime startDate,
        DateTime endDate,
        bool appliesToAllProducts,
        IReadOnlyList<int> productIds)
    {
        if (discountPercentage <= 0 || discountPercentage > 100)
        {
            return new ResponseAPI(400, "Discount percentage must be greater than 0 and at most 100.");
        }

        if (startDate >= endDate)
        {
            return new ResponseAPI(400, "Offer start date must be before end date.");
        }

        if (appliesToAllProducts && productIds.Count > 0)
        {
            return new ResponseAPI(400, "Product ids must be empty when the offer applies to all products.");
        }

        if (!appliesToAllProducts && productIds.Count == 0)
        {
            return new ResponseAPI(400, "Product ids are required when the offer does not apply to all products.");
        }

        return null;
    }
}
