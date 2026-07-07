using Elattaba.API.Helper;
using Elattba.Application.Common;
using Elattba.Application.Users;
using Elattba.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _userService.GetAllAsync();
        return ToActionResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto createUserDto)
    {
        var result = await _userService.CreateAsync(createUserDto);
        if (result.Succeeded && result.StatusCode == 201 && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data.UserId },
                new ResponseAPI(result.StatusCode, result.Message, result.Data));
        }

        return ToActionResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto updateUserDto)
    {
        var result = await _userService.UpdateAsync(id, updateUserDto);
        return ToActionResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _userService.DeleteAsync(id);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        var response = new ResponseAPI(result.StatusCode, result.Message, result.Data);

        return result.StatusCode switch
        {
            200 => Ok(response),
            400 => BadRequest(response),
            404 => NotFound(response),
            >= 500 => Problem(statusCode: result.StatusCode, title: "Internal Server Error", detail: result.Message),
            _ => StatusCode(result.StatusCode, response)
        };
    }

    private IActionResult ToActionResult(ServiceResult result)
    {
        var response = new ResponseAPI(result.StatusCode, result.Message);

        return result.StatusCode switch
        {
            200 => Ok(response),
            400 => BadRequest(response),
            404 => NotFound(response),
            >= 500 => Problem(statusCode: result.StatusCode, title: "Internal Server Error", detail: result.Message),
            _ => StatusCode(result.StatusCode, response)
        };
    }
}
