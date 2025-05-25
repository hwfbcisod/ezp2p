namespace EasyP2P.Web.Enums;

/// <summary>
/// Represents the lifecycle states of a Purchase Order Request (POR) based on
/// the Purchase Order Request Lifecycle Management framework.
/// States follow a sequential progression with defined transition criteria.
/// </summary>
public enum PurchaseOrderRequestState
{
    /// <summary>
    /// The purchase order request has been instantiated by an authorized requestor
    /// and contains all required information including item specification,
    /// quantity requirements, and justification documentation.
    /// </summary>
    Created = 1,

    /// <summary>
    /// The purchase order request enters a review phase wherein authorized personnel
    /// with appropriate organizational permissions conduct a comprehensive evaluation.
    /// Includes business justification review, budget verification, and strategic alignment.
    /// </summary>
    PendingApproval = 2,

    /// <summary>
    /// The purchase order request has successfully completed the approval workflow
    /// and is authorized for conversion to a formal Purchase Order.
    /// </summary>
    Approved = 3,

    /// <summary>
    /// The purchase order request has been declined during the approval process
    /// and will not proceed to Purchase Order creation. This is a terminal state.
    /// </summary>
    Rejected = 4,

    /// <summary>
    /// A formal Purchase Order has been generated based on this approved request,
    /// transitioning the procurement process to the next phase.
    /// </summary>
    PurchaseOrderCreated = 5,

    /// <summary>
    /// The purchase order request has been terminated at any point within the process
    /// lifecycle. May result from business requirement changes, budget constraints,
    /// or strategic organizational decisions.
    /// </summary>
    Cancelled = 6
}