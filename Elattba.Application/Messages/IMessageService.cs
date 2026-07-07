using Elattba.Application.Common;
using Elattba.Core.DTOs;

namespace Elattba.Application.Messages;

public interface IMessageService
{
    Task<ServiceResult<IReadOnlyList<MessageDto>>> GetAllAsync();
    Task<ServiceResult<MessageDto>> GetByIdAsync(int id);
    Task<ServiceResult<MessageDto>> CreateAsync(CreateMessageDto dto);
    Task<ServiceResult<MessageDto>> MarkAsReadAsync(int id, MarkMessageAsReadDto dto);
    Task<ServiceResult> DeleteAsync(int id);
}
