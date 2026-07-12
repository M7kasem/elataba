using Elattaba.API.Helper;
using Elattaba.API.Services;
using Elattba.Application.Auth;
using Elattba.Application.Users;
using Elattba.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserProvisioningService _userProvisioningService;

    public UserController(
        IUserService userService,
        IUserProvisioningService userProvisioningService)
    {
        _userService = userService;
        _userProvisioningService = userProvisioningService;
    }

    [HttpGet]
    [Authorize(Policy = AuthConstants.AdminOnlyPolicy)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _userService.GetAllAsync();
        return this.ToActionResult(result);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthConstants.AdminOnlyPolicy)]
    public async Task<IActionResult> Create([FromBody] CreateUserDto createUserDto)
    {
        var result = await _userProvisioningService.CreateByAdminAsync(createUserDto);
        if (!result.Succeeded || result.Data == null)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data.UserId },
            new ResponseAPI(result.StatusCode, result.Message, result.Data));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto updateUserDto)
    {
        var result = await _userService.UpdateAsync(id, updateUserDto);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AuthConstants.AdminOnlyPolicy)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _userService.DeleteAsync(id);
        return this.ToActionResult(result);
    }

}
