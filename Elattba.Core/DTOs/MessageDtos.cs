namespace Elattba.Core.DTOs;

public record MessageDto(
    int MessageId,
    int SenderId,
    string? SenderEmail,
    int RecipientId,
    string? RecipientEmail,
    int? ProductId,
    string? ProductName,
    string MessageText,
    DateTime SentAt,
    bool IsRead);

public record CreateMessageDto(
    int SenderId,
    int RecipientId,
    int? ProductId,
    string MessageText);

public record MarkMessageAsReadDto(
    bool IsRead);
