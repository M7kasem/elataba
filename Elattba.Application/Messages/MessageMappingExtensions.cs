using ElAtaba.Domain.Entities;
using Elattba.Core.DTOs;

namespace Elattba.Application.Messages;

internal static class MessageMappingExtensions
{
    public static MessageDto ToMessageDto(this Message message) =>
        new(
            message.MessageId,
            message.SenderId,
            message.Sender?.Email,
            message.RecipientId,
            message.Recipient?.Email,
            message.ProductId,
            message.Product?.Name,
            message.MessageText,
            message.SentAt,
            message.IsRead);
}
