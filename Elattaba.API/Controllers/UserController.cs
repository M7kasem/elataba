using System.Security.Cryptography;
using System.Text;
using ElAtaba.Domain.Entities;
using Elattaba.API.Helper;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : BaseController
{
    public UserController(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var users = await _unitOfWork.Users.GetAllAsync(user => user.Governorate!);
            var data = users.Select(user => user.ToDto()).ToList();
            return Ok(new ResponseAPI(200, "Users retrieved successfully", data));
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
            var users = await _unitOfWork.Users.GetAllAsync(user => user.Governorate!);
            var user = users.FirstOrDefault(item => item.UserId == id);
            if (user == null)
            {
                return NotFound(new ResponseAPI(404, "User not found."));
            }

            return Ok(new ResponseAPI(200, "User retrieved successfully", user.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            var governorate = await _unitOfWork.Governorates.GetByIdAsync(createUserDto.GovernorateId);
            if (governorate == null)
            {
                return NotFound(new ResponseAPI(404, "Governorate not found."));
            }

            var user = new User
            {
                Email = createUserDto.Email,
                PasswordHash = HashPassword(createUserDto.Password),
                Phone = createUserDto.Phone,
                Role = createUserDto.Role,
                GovernorateId = createUserDto.GovernorateId,
                City = createUserDto.City,
                ShippingAddress = createUserDto.ShippingAddress
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            user.Governorate = governorate;
            return CreatedAtAction(
                nameof(GetById),
                new { id = user.UserId },
                new ResponseAPI(201, "User created successfully", user.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto updateUserDto)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new ResponseAPI(404, "User not found."));
            }

            var governorate = await _unitOfWork.Governorates.GetByIdAsync(updateUserDto.GovernorateId);
            if (governorate == null)
            {
                return NotFound(new ResponseAPI(404, "Governorate not found."));
            }

            user.Email = updateUserDto.Email;
            user.Phone = updateUserDto.Phone;
            user.Role = updateUserDto.Role;
            user.GovernorateId = updateUserDto.GovernorateId;
            user.City = updateUserDto.City;
            user.ShippingAddress = updateUserDto.ShippingAddress;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            user.Governorate = governorate;
            return Ok(new ResponseAPI(200, "User updated successfully", user.ToDto()));
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
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new ResponseAPI(404, "User not found."));
            }

            await _unitOfWork.Users.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return Ok(new ResponseAPI(200, "User deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }
}
