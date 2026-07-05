using ElAtaba.Domain.Entities;
using Elattaba.API.Helper;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MessageController : BaseController
{
    public MessageController(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var messages = await _unitOfWork.Messages.GetAllAsync(
                message => message.Sender!,
                message => message.Recipient!,
                message => message.Product!);
            var data = messages.Select(message => message.ToDto()).ToList();
            return Ok(new ResponseAPI(200, "Messages retrieved successfully", data));
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
            var messages = await _unitOfWork.Messages.GetAllAsync(
                message => message.Sender!,
                message => message.Recipient!,
                message => message.Product!);
            var message = messages.FirstOrDefault(item => item.MessageId == id);
            if (message == null)
            {
                return NotFound(new ResponseAPI(404, "Message not found."));
            }

            return Ok(new ResponseAPI(200, "Message retrieved successfully", message.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMessageDto createMessageDto)
    {
        try
        {
            var sender = await _unitOfWork.Users.GetByIdAsync(createMessageDto.SenderId);
            if (sender == null)
            {
                return NotFound(new ResponseAPI(404, "Sender not found."));
            }

            var recipient = await _unitOfWork.Users.GetByIdAsync(createMessageDto.RecipientId);
            if (recipient == null)
            {
                return NotFound(new ResponseAPI(404, "Recipient not found."));
            }

            Product? product = null;
            if (createMessageDto.ProductId.HasValue)
            {
                product = await _unitOfWork.Products.GetByIdAsync(createMessageDto.ProductId.Value);
                if (product == null)
                {
                    return NotFound(new ResponseAPI(404, "Product not found."));
                }
            }

            var message = new Message
            {
                SenderId = createMessageDto.SenderId,
                RecipientId = createMessageDto.RecipientId,
                ProductId = createMessageDto.ProductId,
                MessageText = createMessageDto.MessageText
            };

            await _unitOfWork.Messages.AddAsync(message);
            await _unitOfWork.CompleteAsync();

            message.Sender = sender;
            message.Recipient = recipient;
            message.Product = product;
            return CreatedAtAction(
                nameof(GetById),
                new { id = message.MessageId },
                new ResponseAPI(201, "Message created successfully", message.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id, [FromBody] MarkMessageAsReadDto markMessageAsReadDto)
    {
        try
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(id);
            if (message == null)
            {
                return NotFound(new ResponseAPI(404, "Message not found."));
            }

            message.IsRead = markMessageAsReadDto.IsRead;

            await _unitOfWork.Messages.UpdateAsync(message);
            await _unitOfWork.CompleteAsync();

            return Ok(new ResponseAPI(200, "Message read state updated successfully", message.ToDto()));
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
            var message = await _unitOfWork.Messages.GetByIdAsync(id);
            if (message == null)
            {
                return NotFound(new ResponseAPI(404, "Message not found."));
            }

            await _unitOfWork.Messages.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return Ok(new ResponseAPI(200, "Message deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }
}
