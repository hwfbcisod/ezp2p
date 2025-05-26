using EasyP2P.Web.Enums;

namespace EasyP2P.Web.Services;

public static class PermissionMatrix
{
    private static readonly Dictionary<UserRole, HashSet<string>> _permissions = new()
    {
        [UserRole.Requestor] = new()
        {
            "CreatePOR", "CancelOwnPOR", "ViewOwnPOR"
        },
        [UserRole.Approver] = new()
        {
            "CreatePOR", "CancelOwnPOR", "ViewOwnPOR",
            "ApprovePOR", "RejectPOR", "ViewAllPOR",
            "ApprovePO", "RejectPO", "ViewAllPO"
        },
        [UserRole.Purchaser] = new()
        {
            "CreatePOR", "CancelOwnPOR", "ViewOwnPOR", "ViewAllPOR",
            "CreatePO", "SendPO", "ReceivePO", "ProcessInvoice", "ViewAllPO",
            "CreateSupplier", "EditSupplier", "ViewAllSuppliers"
        },
        [UserRole.Administrator] = new()
        {
            "*" // All permissions
        }
    };

    private static readonly Dictionary<(UserRole, string, string, string), bool> _transitions = new()
    {
        // POR transitions
        [(UserRole.Approver, "PendingApproval", "Approved", "POR")] = true,
        [(UserRole.Approver, "PendingApproval", "Rejected", "POR")] = true,
        [(UserRole.Requestor, "Created", "Cancelled", "POR")] = true,
        [(UserRole.Requestor, "PendingApproval", "Cancelled", "POR")] = true,
        [(UserRole.Purchaser, "Approved", "PurchaseOrderCreated", "POR")] = true,

        // PO transitions  
        [(UserRole.Purchaser, "Created", "PendingApproval", "PO")] = true,
        [(UserRole.Approver, "PendingApproval", "Approved", "PO")] = true,
        [(UserRole.Approver, "PendingApproval", "Rejected", "PO")] = true,
        [(UserRole.Purchaser, "Approved", "Sent", "PO")] = true,
        [(UserRole.Purchaser, "Sent", "Acknowledged", "PO")] = true,
        [(UserRole.Purchaser, "Acknowledged", "PartiallyReceived", "PO")] = true,
        [(UserRole.Purchaser, "Acknowledged", "FullyReceived", "PO")] = true,
        [(UserRole.Purchaser, "PartiallyReceived", "FullyReceived", "PO")] = true,
        [(UserRole.Purchaser, "FullyReceived", "Invoiced", "PO")] = true,
        [(UserRole.Purchaser, "Invoiced", "ThreeWayMatch", "PO")] = true,
        [(UserRole.Administrator, "ThreeWayMatch", "PaymentMade", "PO")] = true,
        [(UserRole.Administrator, "PaymentMade", "ClosedCompleted", "PO")] = true,
    };

    public static bool HasPermission(UserRole role, string action)
    {
        if (_permissions.TryGetValue(role, out var permissions))
        {
            return permissions.Contains("*") || permissions.Contains(action);
        }
        return false;
    }

    public static bool CanTransition(UserRole role, string fromState, string toState, string entityType)
    {
        // Administrator can do everything
        if (role == UserRole.Administrator)
            return true;

        return _transitions.ContainsKey((role, fromState, toState, entityType));
    }
}