using ElAtaba.Domain.Enums;

namespace ElAtaba.Domain.Entities;

/// <summary>
/// Every platform user - buyers and sellers share this single table.
/// </summary>
public class User
{
    public int UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    /// <summary>Optional - messaging exists on the platform, so a phone number isn't required.</summary>
    public string? Phone { get; set; }

    public UserRole Role { get; set; } = UserRole.Buyer;

    public int GovernorateId { get; set; }
    public Governorate? Governorate { get; set; }

    public string City { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;

    public string? ProfilePictureUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Store? OwnedStore { get; set; }
    public ICollection<Store> ManagedStores { get; set; } = new List<Store>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
}
