using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyP2P.Infrastructure.Domain.Events;

/// <summary>
/// Base class for all domain events
/// </summary>
public abstract class DomainEvent
{
    public Guid Id { get; private set; }
    public Guid ProcessId { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public int Version { get; private set; }
    public long SequenceNumber { get; private set; } // Will be set by the repository

    protected DomainEvent()
    {
        // For deserialization
    }

    protected DomainEvent(Guid processId)
    {
        Id = Guid.NewGuid();
        ProcessId = processId;
        OccurredAt = DateTime.UtcNow;
    }

    // Helper method to set the version when loading from the repository
    internal void SetVersion(int version)
    {
        Version = version;
    }

    // Helper method to set the sequence number when loading from the repository
    internal void SetSequenceNumber(long sequenceNumber)
    {
        SequenceNumber = sequenceNumber;
    }
}

/// <summary>
/// Event raised when a new purchase order process starts
/// </summary>
public class ProcessStartedEvent : DomainEvent
{
    private ProcessStartedEvent() : base() { }

    public ProcessStartedEvent(Guid processId) : base(processId)
    {
    }
}

/// <summary>
/// Event raised when a purchase order request approval task is created
/// </summary>
public class PurchaseOrderRequestApprovalTaskCreatedEvent : DomainEvent
{
    public string TaskTitle { get; private set; }
    public string TaskDescription { get; private set; }

    private PurchaseOrderRequestApprovalTaskCreatedEvent() : base() { }

    public PurchaseOrderRequestApprovalTaskCreatedEvent(
        Guid processId,
        string taskTitle,
        string taskDescription) : base(processId)
    {
        TaskTitle = taskTitle;
        TaskDescription = taskDescription;
    }
}

/// <summary>
/// Event raised when a purchase order request is approved
/// </summary>
public class PurchaseOrderRequestApprovedEvent : DomainEvent
{
    public Guid TaskId { get; private set; }
    public string ApprovedBy { get; private set; }

    private PurchaseOrderRequestApprovedEvent() : base() { }

    public PurchaseOrderRequestApprovedEvent(
        Guid processId,
        Guid taskId,
        string approvedBy = null) : base(processId)
    {
        TaskId = taskId;
        ApprovedBy = approvedBy;
    }
}

/// <summary>
/// Event raised when a purchase order request is rejected
/// </summary>
public class PurchaseOrderRequestRejectedEvent : DomainEvent
{
    public Guid TaskId { get; private set; }
    public string RejectedBy { get; private set; }
    public string RejectionReason { get; private set; }

    private PurchaseOrderRequestRejectedEvent() : base() { }

    public PurchaseOrderRequestRejectedEvent(
        Guid processId,
        Guid taskId,
        string rejectedBy = null,
        string rejectionReason = null) : base(processId)
    {
        TaskId = taskId;
        RejectedBy = rejectedBy;
        RejectionReason = rejectionReason;
    }
}

/// <summary>
/// Event raised when a purchase order creation task is created
/// </summary>
public class PurchaseOrderCreationTaskCreatedEvent : DomainEvent
{
    public string TaskTitle { get; private set; }
    public string TaskDescription { get; private set; }

    private PurchaseOrderCreationTaskCreatedEvent() : base() { }

    public PurchaseOrderCreationTaskCreatedEvent(
        Guid processId,
        string taskTitle,
        string taskDescription) : base(processId)
    {
        TaskTitle = taskTitle;
        TaskDescription = taskDescription;
    }
}

/// <summary>
/// Event raised when a purchase order is created
/// </summary>
public class PurchaseOrderCreatedEvent : DomainEvent
{
    public Guid TaskId { get; private set; }
    public string PurchaseOrderNumber { get; private set; }
    public string CreatedBy { get; private set; }

    private PurchaseOrderCreatedEvent() : base() { }

    public PurchaseOrderCreatedEvent(
        Guid processId,
        Guid taskId,
        string purchaseOrderNumber,
        string createdBy = null) : base(processId)
    {
        TaskId = taskId;
        PurchaseOrderNumber = purchaseOrderNumber;
        CreatedBy = createdBy;
    }
}

/// <summary>
/// Event raised when a purchase order approval task is created
/// </summary>
public class PurchaseOrderApprovalTaskCreatedEvent : DomainEvent
{
    public string TaskTitle { get; private set; }
    public string TaskDescription { get; private set; }
    public string PurchaseOrderNumber { get; private set; }

    private PurchaseOrderApprovalTaskCreatedEvent() : base() { }

    public PurchaseOrderApprovalTaskCreatedEvent(
        Guid processId,
        string taskTitle,
        string taskDescription,
        string purchaseOrderNumber) : base(processId)
    {
        TaskTitle = taskTitle;
        TaskDescription = taskDescription;
        PurchaseOrderNumber = purchaseOrderNumber;
    }
}

/// <summary>
/// Event raised when a purchase order is approved
/// </summary>
public class PurchaseOrderApprovedEvent : DomainEvent
{
    public Guid TaskId { get; private set; }
    public string PurchaseOrderNumber { get; private set; }
    public string ApprovedBy { get; private set; }

    private PurchaseOrderApprovedEvent() : base() { }

    public PurchaseOrderApprovedEvent(
        Guid processId,
        Guid taskId,
        string purchaseOrderNumber,
        string approvedBy = null) : base(processId)
    {
        TaskId = taskId;
        PurchaseOrderNumber = purchaseOrderNumber;
        ApprovedBy = approvedBy;
    }
}

/// <summary>
/// Event raised when a three-way match task is created
/// </summary>
public class ThreeWayMatchTaskCreatedEvent : DomainEvent
{
    public string TaskTitle { get; private set; }
    public string TaskDescription { get; private set; }
    public string PurchaseOrderNumber { get; private set; }
    public string InvoiceNumber { get; private set; }

    private ThreeWayMatchTaskCreatedEvent() : base() { }

    public ThreeWayMatchTaskCreatedEvent(
        Guid processId,
        string taskTitle,
        string taskDescription,
        string purchaseOrderNumber,
        string invoiceNumber = null) : base(processId)
    {
        TaskTitle = taskTitle;
        TaskDescription = taskDescription;
        PurchaseOrderNumber = purchaseOrderNumber;
        InvoiceNumber = invoiceNumber;
    }
}

/// <summary>
/// Event raised when a three-way match is completed
/// </summary>
public class ThreeWayMatchCompletedEvent : DomainEvent
{
    public Guid TaskId { get; private set; }
    public string PurchaseOrderNumber { get; private set; }
    public string InvoiceNumber { get; private set; }
    public bool IsMatched { get; private set; }
    public string CompletedBy { get; private set; }

    private ThreeWayMatchCompletedEvent() : base() { }

    public ThreeWayMatchCompletedEvent(
        Guid processId,
        Guid taskId,
        string purchaseOrderNumber,
        string invoiceNumber,
        bool isMatched,
        string completedBy = null) : base(processId)
    {
        TaskId = taskId;
        PurchaseOrderNumber = purchaseOrderNumber;
        InvoiceNumber = invoiceNumber;
        IsMatched = isMatched;
        CompletedBy = completedBy;
    }
}

/// <summary>
/// Event raised when a payment approval task is created
/// </summary>
public class PaymentApprovalTaskCreatedEvent : DomainEvent
{
    public string TaskTitle { get; private set; }
    public string TaskDescription { get; private set; }
    public string InvoiceNumber { get; private set; }
    public decimal Amount { get; private set; }

    private PaymentApprovalTaskCreatedEvent() : base() { }

    public PaymentApprovalTaskCreatedEvent(
        Guid processId,
        string taskTitle,
        string taskDescription,
        string invoiceNumber,
        decimal amount) : base(processId)
    {
        TaskTitle = taskTitle;
        TaskDescription = taskDescription;
        InvoiceNumber = invoiceNumber;
        Amount = amount;
    }
}

/// <summary>
/// Event raised when a payment is approved
/// </summary>
public class PaymentApprovedEvent : DomainEvent
{
    public Guid TaskId { get; private set; }
    public string InvoiceNumber { get; private set; }
    public decimal Amount { get; private set; }
    public string ApprovedBy { get; private set; }

    private PaymentApprovedEvent() : base() { }

    public PaymentApprovedEvent(
        Guid processId,
        Guid taskId,
        string invoiceNumber,
        decimal amount,
        string approvedBy = null) : base(processId)
    {
        TaskId = taskId;
        InvoiceNumber = invoiceNumber;
        Amount = amount;
        ApprovedBy = approvedBy;
    }
}

/// <summary>
/// Event raised when a payment is executed
/// </summary>
public class PaymentExecutedEvent : DomainEvent
{
    public string InvoiceNumber { get; private set; }
    public decimal Amount { get; private set; }
    public string PaymentReference { get; private set; }
    public DateTime PaymentDate { get; private set; }

    private PaymentExecutedEvent() : base() { }

    public PaymentExecutedEvent(
        Guid processId,
        string invoiceNumber,
        decimal amount,
        string paymentReference) : base(processId)
    {
        InvoiceNumber = invoiceNumber;
        Amount = amount;
        PaymentReference = paymentReference;
        PaymentDate = DateTime.UtcNow;
    }
}

/// <summary>
/// Event raised when a process is completed
/// </summary>
public class ProcessCompletedEvent : DomainEvent
{
    public bool IsSuccessful { get; private set; }
    public string CompletionReason { get; private set; }

    private ProcessCompletedEvent() : base() { }

    public ProcessCompletedEvent(
        Guid processId,
        bool isSuccessful,
        string completionReason = null) : base(processId)
    {
        IsSuccessful = isSuccessful;
        CompletionReason = completionReason;
    }
}

/// <summary>
/// Event raised when a process fails
/// </summary>
public class ProcessFailedEvent : DomainEvent
{
    public string FailureReason { get; private set; }
    public string ErrorDetails { get; private set; }

    private ProcessFailedEvent() : base() { }

    public ProcessFailedEvent(
        Guid processId,
        string failureReason,
        string errorDetails = null) : base(processId)
    {
        FailureReason = failureReason;
        ErrorDetails = errorDetails;
    }
}