//using EasyP2P.Infrastructure.Domain.Events;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace EasyP2P.Infrastructure;

///// <summary>
///// ProcessAggregate represents an event-sourced purchase order process
///// It stores and applies domain events that occur during the process lifecycle
///// </summary>
//public class ProcessAggregate
//{
//    private readonly List<DomainEvent> _events = new List<DomainEvent>();
//    private int _version = 0;

//    public Guid Id { get; private set; }
//    public State CurrentState { get; private set; }

//    public string PurchaseOrderNumber { get; private set; }
//    public string InvoiceNumber { get; private set; }
//    public decimal? InvoiceAmount { get; private set; }
//    public string PaymentReference { get; private set; }
//    public bool IsCompleted { get; private set; }

//    public ProcessAggregate(Guid id)
//    {
//        Id = id;
//        CurrentState = State.NotStarted;
//    }

//    public void Apply(DomainEvent @event)
//    {
//        ApplyEvent(@event);

//        _events.Add(@event);
//    }

//    public void Apply(IOrderedEnumerable<DomainEvent> events)
//    {
//        foreach (var @event in events)
//        {
//            ApplyEvent(@event);
//            _version = @event.Version;
//        }
//    }

//    private void ApplyEvent(DomainEvent @event)
//    {
//        switch (@event)
//        {
//            case ProcessStartedEvent:
//                CurrentState = State.Executing;
//                break;

//            case PurchaseOrderRequestApprovalTaskCreatedEvent:
//                CurrentState = State.Hibernated;
//                break;

//            case PurchaseOrderRequestApprovedEvent:
//                break;

//            case PurchaseOrderCreationTaskCreatedEvent:
//                break;

//            case PurchaseOrderCreatedEvent createdEvent:
//                PurchaseOrderNumber = createdEvent.PurchaseOrderNumber;
//                break;

//            case PurchaseOrderApprovalTaskCreatedEvent:
//                break;

//            case PurchaseOrderApprovedEvent:
//                break;

//            case ThreeWayMatchTaskCreatedEvent threeWayMatchEvent:
//                if (string.IsNullOrEmpty(InvoiceNumber))
//                {
//                    InvoiceNumber = threeWayMatchEvent.InvoiceNumber;
//                }
//                break;

//            case ThreeWayMatchCompletedEvent:
//                break;

//            case PaymentApprovalTaskCreatedEvent paymentEvent:
//                InvoiceAmount = paymentEvent.Amount;
//                break;

//            case PaymentApprovedEvent:
//                break;

//            case PaymentExecutedEvent paymentEvent:
//                PaymentReference = paymentEvent.PaymentReference;
//                CurrentState = State.Finished;
//                break;

//            case ProcessCompletedEvent completedEvent:
//                IsCompleted = true;
//                CurrentState = State.Finished;
//                break;

//            case ProcessFailedEvent:
//                CurrentState = State.Finished;
//                IsCompleted = true;
//                break;
//        }
//    }

//    public IReadOnlyList<DomainEvent> GetEvents()
//    {
//        return _events.AsReadOnly();
//    }

//    public int GetVersion()
//    {
//        return _version;
//    }
//}