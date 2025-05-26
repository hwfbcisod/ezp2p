namespace EasyP2P.Web.Enums;

/// <summary>
/// Represents the lifecycle states of a purchase order based on the 
/// Purchase Order Lifecycle Management framework.
/// States follow a sequential progression with defined transition criteria.
/// </summary>
public enum PurchaseOrderState
{
    /// <summary>
    /// The purchase order is instantiated based on a previously approved 
    /// Purchase Order Request (POR). Critical procurement decisions are finalized,
    /// including supplier selection and comprehensive price calculation.
    /// </summary>
    Created = 1,

    /// <summary>
    /// The purchase order enters a review phase wherein authorized personnel 
    /// with appropriate organizational permissions conduct a comprehensive evaluation.
    /// Includes price verification, supplier assessment, and strategic alignment.
    /// </summary>
    PendingApproval = 2,

    /// <summary>
    /// The purchase order has successfully completed the approval workflow 
    /// and is authorized for transmission to the designated supplier.
    /// </summary>
    Approved = 3,

    /// <summary>
    /// The purchase order has been declined during the approval process 
    /// and will not proceed to supplier transmission. This is a terminal state.
    /// </summary>
    Rejected = 4,

    /// <summary>
    /// The purchase order has been formally transmitted to the vendor 
    /// through established communication channels, constituting an official 
    /// request for goods or services.
    /// </summary>
    Sent = 5,

    /// <summary>
    /// A subset of the ordered items has been delivered and formally received
    /// by the organization, while the complete order remains unfulfilled.
    /// Accommodates multi-line orders and phased delivery schedules.
    /// </summary>
    PartiallyReceived = 6,

    /// <summary>
    /// All ordered items have been delivered in accordance with the purchase
    /// order specifications. Delivery documentation is formally attached 
    /// to the procurement record.
    /// </summary>
    FullyReceived = 7,

    /// <summary>
    /// The vendor has submitted the corresponding invoice for payment processing,
    /// with the invoice documentation integrated into the procurement process workflow.
    /// </summary>
    Invoiced = 8,

    /// <summary>
    /// A systematic comparison is conducted between the original purchase order,
    /// the delivery note, and the vendor invoice. This control mechanism verifies
    /// alignment between ordered quantities, delivered quantities, and invoiced amounts.
    /// </summary>
    ThreeWayMatch = 9,

    /// <summary>
    /// Financial settlement has been executed in accordance with the approved
    /// invoice and organizational payment terms. Funds have been transferred
    /// to the supplier through established payment mechanisms.
    /// </summary>
    PaymentMade = 10,

    /// <summary>
    /// The supplier has confirmed receipt of payment, and all procedural
    /// requirements have been satisfied. This terminal state represents
    /// successful completion of the entire procurement cycle.
    /// </summary>
    Completed = 11,

    /// <summary>
    /// The purchase order has been terminated at any point within the process
    /// lifecycle. May result from business requirement changes, supplier
    /// performance issues, delivery complications, or strategic organizational decisions.
    /// </summary>
    Cancelled = 12
}