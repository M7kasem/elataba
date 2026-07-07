using Microsoft.AspNetCore.Identity;

namespace Elattba.InfraStructure.Identity;

public sealed class AppUser : IdentityUser
{
    public int DomainUserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int? StoreId { get; set; }
}
