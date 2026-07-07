using ElAtaba.Domain.Entities;
using Elattba.Application.Common;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;

namespace Elattba.Application.Messages;

public sealed class MessageService : IMessageService
{
    private readonly IUnitOfWork _unitOfWork;

    public MessageService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<IReadOnlyList<MessageDto>>> GetAllAsync()
    {
        try
        {
            var messages = await _unitOfWork.Messages.ListAsync(
                null,
                true,
                message => message.Sender!,
                message => message.Recipient!,
                message => message.Product!);
            var data = messages.Select(message => message.ToMessageDto()).ToList();

            return new ServiceResult<IReadOnlyList<MessageDto>>(true, 200, "Messages retrieved successfully", data);
        }
        catch (Exception ex)
        {
            return Failure<IReadOnlyList<MessageDto>>(ex);
        }
    }

    public async Task<ServiceResult<MessageDto>> GetByIdAsync(int id)
    {
        try
        {
            var message = await GetMessageWithDetailsAsync(id, disableTracking: true);
            if (message == null)
            {
                return new ServiceResult<MessageDto>(false, 404, "Message not found.");
            }

            return new ServiceResult<MessageDto>(true, 200, "Message retrieved successfully", message.ToMessageDto());
        }
        catch (Exception ex)
        {
            return Failure<MessageDto>(ex);
        }
    }

    public async Task<ServiceResult<MessageDto>> CreateAsync(CreateMessageDto dto)
    {
        try
        {
            var sender = await _unitOfWork.Users.GetByIdAsync(dto.SenderId);
            if (sender == null)
            {
                return new ServiceResult<MessageDto>(false, 404, "Sender not found.");
            }

            var recipient = await _unitOfWork.Users.GetByIdAsync(dto.RecipientId);
            if (recipient == null)
            {
                return new ServiceResult<MessageDto>(false, 404, "Recipient not found.");
            }

            Product? product = null;
            if (dto.ProductId.HasValue)
            {
                product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId.Value);
                if (product == null)
                {
                    return new ServiceResult<MessageDto>(false, 404, "Product not found.");
                }
            }

            var message = new Message
            {
                SenderId = dto.SenderId,
                RecipientId = dto.RecipientId,
                ProductId = dto.ProductId,
                MessageText = dto.MessageText
            };

            await _unitOfWork.Messages.AddAsync(message);
            await _unitOfWork.CompleteAsync();

            message.Sender = sender;
            message.Recipient = recipient;
            message.Product = product;
            return new ServiceResult<MessageDto>(true, 201, "Message created successfully", message.ToMessageDto());
        }
        catch (Exception ex)
        {
            return Failure<MessageDto>(ex);
        }
    }

    public async Task<ServiceResult<MessageDto>> MarkAsReadAsync(int id, MarkMessageAsReadDto dto)
    {
        try
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(id);
            if (message == null)
            {
                return new ServiceResult<MessageDto>(false, 404, "Message not found.");
            }

            message.IsRead = dto.IsRead;

            await _unitOfWork.Messages.UpdateAsync(message);
            await _unitOfWork.CompleteAsync();

            var updatedMessage = await GetMessageWithDetailsAsync(id, disableTracking: true) ?? message;
            return new ServiceResult<MessageDto>(true, 200, "Message read state updated successfully", updatedMessage.ToMessageDto());
        }
        catch (Exception ex)
        {
            return Failure<MessageDto>(ex);
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        try
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(id);
            if (message == null)
            {
                return new ServiceResult(false, 404, "Message not found.");
            }

            await _unitOfWork.Messages.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult(true, 200, "Message deleted successfully");
        }
        catch (Exception)
        {
            return new ServiceResult(false, 500, "Unexpected server error.");
        }
    }

    private Task<Message?> GetMessageWithDetailsAsync(int id, bool disableTracking)
    {
        return _unitOfWork.Messages.GetFirstOrDefaultAsync(
            message => message.MessageId == id,
            disableTracking,
            message => message.Sender!,
            message => message.Recipient!,
            message => message.Product!);
    }

    private static ServiceResult<T> Failure<T>(Exception ex) =>
        new(false, 500, "Unexpected server error.");
}
