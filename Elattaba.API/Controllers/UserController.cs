using Elattaba.API.Helper;
using Elattaba.API.Services;
using Elattba.Application.Auth;
using Elattba.Application.Users;
using Elattba.Core.DTOs;
using Elattba.InfraStructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly UserManager<AppUser> _userManager;

    public UserController(
        IUserService userService,
        IUserProvisioningService userProvisioningService,
        UserManager<AppUser> userManager)
    {
        _userService = userService;
        _userProvisioningService = userProvisioningService;
        _userManager = userManager;
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
        if (!result.Succeeded || result.Data == null)
        {
            return this.ToActionResult(result);
        }

        var appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.DomainUserId == id);
        if (appUser != null)
        {
            var enrichedDto = result.Data with 
            { 
                FirstName = appUser.FirstName, 
                LastName = appUser.LastName 
            };
            return Ok(new ResponseAPI(result.StatusCode, result.Message, enrichedDto));
        }

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
        if (!result.Succeeded || result.Data == null)
        {
            return this.ToActionResult(result);
        }

        var appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.DomainUserId == id);
        if (appUser != null)
        {
            appUser.FirstName = updateUserDto.FirstName;
            appUser.LastName = updateUserDto.LastName;
            appUser.PhoneNumber = updateUserDto.Phone;
            appUser.Email = updateUserDto.Email;
            appUser.NormalizedEmail = updateUserDto.Email.ToUpperInvariant();
            appUser.UserName = updateUserDto.Email;
            appUser.NormalizedUserName = updateUserDto.Email.ToUpperInvariant();

            var updateIdentityResult = await _userManager.UpdateAsync(appUser);
            if (!updateIdentityResult.Succeeded)
            {
                var errors = string.Join(" ", updateIdentityResult.Errors.Select(e => e.Description));
                return BadRequest(new ResponseAPI(400, $"Failed to update Identity account: {errors}"));
            }

            var enrichedDto = result.Data with 
            { 
                FirstName = appUser.FirstName, 
                LastName = appUser.LastName 
            };
            return Ok(new ResponseAPI(result.StatusCode, result.Message, enrichedDto));
        }

        return this.ToActionResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AuthConstants.AdminOnlyPolicy)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _userService.DeleteAsync(id);
        return this.ToActionResult(result);
    }

    [HttpPut("{id}/profile-picture")]
    [Authorize]
    public async Task<IActionResult> UploadProfilePicture(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new ResponseAPI(400, "No file uploaded."));

        var imageUploadFile = new Elattba.Core.Services.ImageUploadFile(
            file.OpenReadStream(),
            file.FileName,
            file.ContentType,
            file.Length);

        var result = await _userService.UploadProfilePictureAsync(id, imageUploadFile);
        return this.ToActionResult(result);
    }
}
