namespace ElAtaba.Domain.Entities;

/// <summary>Direct messaging between users (buyer &lt;-&gt; seller communication).</summary>
public class Message
{
    public int MessageId { get; set; }

    public int SenderId { get; set; }
    public User? Sender { get; set; }

    public int RecipientId { get; set; }
    public User? Recipient { get; set; }

    /// <summary>Optional context, e.g. "asking about this product".</summary>
    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    public string MessageText { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
}
