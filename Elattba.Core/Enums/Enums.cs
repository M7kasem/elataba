namespace ElAtaba.Domain.Enums;

/// <summary>
/// The role a user plays on the platform.
/// Phase 2 will add "Reseller" as a new value here only -
/// no existing Buyer/Seller/Admin logic needs to change.
/// </summary>
public enum UserRole
{
    Buyer = 0,
    Seller = 1,
    Admin = 2,
    StoreManager = 3
}

/// <summary>Lifecycle of an order. Kept minimal for the MVP.</summary>
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}

/// <summary>How the order will be paid for. Online is reserved for a future payment gateway.</summary>
public enum PaymentMethod
{
    Cash = 0,
    Online = 1
}

/// <summary>Whether the money for the order has actually been collected.</summary>
public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2
}
