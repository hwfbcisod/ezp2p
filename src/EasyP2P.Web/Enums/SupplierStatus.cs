namespace EasyP2P.Web.Enums;

/// <summary>
/// Represents the possible statuses of a supplier.
/// </summary>
public enum SupplierStatus
{
    /// <summary>
    /// Supplier is active and can receive purchase orders.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Supplier is temporarily inactive.
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// Supplier is pending approval/verification.
    /// </summary>
    Pending = 3,

    /// <summary>
    /// Supplier is suspended due to performance issues.
    /// </summary>
    Suspended = 4
}