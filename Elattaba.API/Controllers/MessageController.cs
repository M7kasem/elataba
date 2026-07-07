using Elattaba.API.Helper;
using Elattba.Application.Common;
using Elattba.Application.Messages;
using Elattba.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessageController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _messageService.GetAllAsync();
        return this.ToActionResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _messageService.GetByIdAsync(id);
        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMessageDto createMessageDto)
    {
        var result = await _messageService.CreateAsync(createMessageDto);
        if (result.Succeeded && result.StatusCode == 201 && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data.MessageId },
                new ResponseAPI(result.StatusCode, result.Message, result.Data));
        }

        return this.ToActionResult(result);
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id, [FromBody] MarkMessageAsReadDto markMessageAsReadDto)
    {
        var result = await _messageService.MarkAsReadAsync(id, markMessageAsReadDto);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _messageService.DeleteAsync(id);
        return this.ToActionResult(result);
    }

}
