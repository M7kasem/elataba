using Elattaba.API.Helper;
using Elattba.Application.Auth;
using Elattba.Application.Checkouts;
using Elattba.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CheckoutController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;

    public CheckoutController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    [HttpPost]
    [Authorize(Policy = AuthConstants.BuyerOnlyPolicy)]
    public async Task<IActionResult> Create([FromBody] CreateCheckoutDto createCheckoutDto)
    {
        var result = await _checkoutService.CreateAsync(createCheckoutDto);
        if (result.Succeeded && result.StatusCode == 201 && result.Data != null)
        {
            return StatusCode(
                StatusCodes.Status201Created,
                new ResponseAPI(result.StatusCode, result.Message, result.Data));
        }

        return this.ToActionResult(result);
    }
}
