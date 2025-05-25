using EasyP2P.Web.Models;
using EasyP2P.Web.Services;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;

namespace EasyP2P.Web.Controllers;

public class PurchaseOrderController : Controller
{
    private readonly IPurchaseOrderService _purchaseOrderService;
    private readonly IPurchaseOrderRequestService _purchaseOrderRequestService;
    private readonly ILogger<PurchaseOrderController> _logger;

    public PurchaseOrderController(
        IPurchaseOrderService purchaseOrderService,
        IPurchaseOrderRequestService purchaseOrderRequestService,
        ILogger<PurchaseOrderController> logger)
    {
        _purchaseOrderService = purchaseOrderService;
        _purchaseOrderRequestService = purchaseOrderRequestService;
        _logger = logger;
    }

    // GET: PurchaseOrder/Index
    public async Task<IActionResult> Index()
    {
        var orders = await _purchaseOrderService.GetAllOrdersAsync();

        return View(orders);
    }

    // GET: PurchaseOrder/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var order = await _purchaseOrderService.GetOrderByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    // GET: PurchaseOrder/ExportPdf/5
    public async Task<IActionResult> ExportPdf(int id)
    {
        var order = await _purchaseOrderService.GetOrderByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        try
        {
            var pdfBytes = await GeneratePdfAsync(order);
            return File(pdfBytes, "application/pdf", $"PurchaseOrder-{order.Id}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF for purchase order {OrderId}", id);
            TempData["ErrorMessage"] = "An error occurred while generating the PDF. Please try again.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // GET: PurchaseOrder/Create/5 (where 5 is the purchase order request ID)
    public async Task<IActionResult> Create(int id)
    {
        // Get the purchase order request to prefill the form
        var request = await _purchaseOrderRequestService.GetRequestByIdAsync(id);
        if (request == null)
        {
            return NotFound();
        }

        // Business logic validation through service
        if (request.Status != "Approved")
        {
            TempData["ErrorMessage"] = "Only approved purchase order requests can be converted to purchase orders.";
            return RedirectToAction("Index", "PurchaseOrderRequest");
        }

        // Check if a purchase order already exists for this request
        var existingOrders = await _purchaseOrderService.GetOrdersByRequestIdAsync(id);
        if (existingOrders.Any())
        {
            TempData["ErrorMessage"] = "A purchase order has already been created for this request.";
            return RedirectToAction("Index");
        }

        // Create a new model pre-filled with the request data
        var model = new PurchaseOrderModel
        {
            PurchaseOrderRequestId = request.Id,
            ItemName = request.ItemName,
            Quantity = request.Quantity,
            UnitPrice = 0, // User needs to fill this
            TotalPrice = 0, // Will be calculated
            Supplier = "" // User needs to fill this
        };

        return View(model);
    }

    // Enhanced Create method using service layer
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PurchaseOrderModel model)
    {
        if (ModelState.IsValid)
        {
            // Validate using service layer business rules
            var validationResult = await _purchaseOrderService.ValidateOrderAsync(model);

            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError("", error);
                }
            }

            // Add warnings as informational messages
            foreach (var warning in validationResult.Warnings)
            {
                TempData["WarningMessage"] = warning;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string currentUser = "CurrentUser"; // Replace with actual user

                    var orderId = await _purchaseOrderService.CreateOrderAsync(model, currentUser);

                    // Update the corresponding POR status through service layer coordination
                    await _purchaseOrderRequestService.MarkPurchaseOrderCreatedAsync(
                        model.PurchaseOrderRequestId, currentUser);

                    _logger.LogInformation("Purchase order created with ID: {OrderId} for POR: {PorId}",
                        orderId, model.PurchaseOrderRequestId);

                    TempData["SuccessMessage"] = $"Purchase order #{orderId} created successfully.";
                    return RedirectToAction(nameof(Details), new { id = orderId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating purchase order");
                    ModelState.AddModelError("", "An error occurred while creating the purchase order.");
                }
            }
        }

        return View(model);
    }

    // POST: PurchaseOrder/Approve/5
    [HttpPost, ActionName("Approve")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveConfirmed(int id)
    {
        try
        {
            string currentUser = "ApproverUser"; // Replace with actual user

            var success = await _purchaseOrderService.ApproveOrderAsync(id, currentUser);

            if (success)
            {
                TempData["SuccessMessage"] = $"Purchase order #{id} has been approved successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to approve the purchase order. It may not be in the correct state.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving purchase order with ID {OrderId}", id);
            TempData["ErrorMessage"] = "An error occurred while approving the purchase order.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: PurchaseOrder/Reject/5
    [HttpPost, ActionName("Reject")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectConfirmed(int id)
    {
        try
        {
            string currentUser = "ApproverUser"; // Replace with actual user

            var success = await _purchaseOrderService.RejectOrderAsync(id, currentUser);

            if (success)
            {
                TempData["SuccessMessage"] = $"Purchase order #{id} has been rejected.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to reject the purchase order. It may not be in the correct state.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting purchase order with ID {OrderId}", id);
            TempData["ErrorMessage"] = "An error occurred while rejecting the purchase order.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ActionName("Cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelConfirmed(int id)
    {
        try
        {
            string currentUser = "CurrentUser"; // Replace with actual user

            var success = await _purchaseOrderService.CancelOrderAsync(id, currentUser);

            if (success)
            {
                TempData["SuccessMessage"] = "Purchase order cancelled successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Purchase order not found or could not be cancelled.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling purchase order with ID {OrderId}", id);
            TempData["ErrorMessage"] = "An error occurred while cancelling the purchase order.";
        }

        return RedirectToAction(nameof(Index));
    }

    // AJAX action to calculate the total price
    [HttpPost]
    public IActionResult CalculateTotalPrice(int quantity, decimal unitPrice)
    {
        decimal totalPrice = quantity * unitPrice;
        return Json(new { totalPrice = totalPrice.ToString("C") });
    }

    // Private method for PDF generation (extracted from controller logic)
    private async Task<byte[]> GeneratePdfAsync(PurchaseOrderViewModel order)
    {
        // Create a memory stream to write the PDF to
        var memoryStream = new MemoryStream();

        // Configure writer properties to avoid BouncyCastle dependency issues
        var writerProperties = new iText.Kernel.Pdf.WriterProperties();

        // Create a PDF writer and document
        var writer = new PdfWriter(memoryStream, writerProperties);
        var pdf = new PdfDocument(writer);
        var document = new Document(pdf);

        // Set document margins
        document.SetMargins(36, 36, 36, 36);

        // Add title
        var title = new Paragraph($"Purchase Order #{order.Id}")
            .SetFontSize(20)
            .SimulateBold()
            .SetTextAlignment(TextAlignment.CENTER);
        document.Add(title);

        // Add company header
        var companyInfo = new Paragraph("EasyP2P Company")
            .SetFontSize(14)
            .SetTextAlignment(TextAlignment.CENTER);
        document.Add(companyInfo);

        // Add date
        var date = new Paragraph($"Date: {DateTime.Now:yyyy-MM-dd}")
            .SetTextAlignment(TextAlignment.RIGHT);
        document.Add(date);

        // Add space
        document.Add(new Paragraph("\n"));

        // Add order and request info
        document.Add(new Paragraph($"PO #: {order.Id}")
            .SimulateBold());
        document.Add(new Paragraph($"POR #: {order.PurchaseOrderRequestId}"));
        document.Add(new Paragraph($"Status: {order.Status}"));
        document.Add(new Paragraph($"Created by: {order.CreatedBy}"));
        document.Add(new Paragraph($"Order date: {order.OrderDate:yyyy-MM-dd}"));
        document.Add(new Paragraph($"Supplier: {order.Supplier}")
            .SimulateBold());

        // Add space
        document.Add(new Paragraph("\n"));

        // Create table for item details
        var table = new Table(4)
            .UseAllAvailableWidth()
            .SetBorder(new iText.Layout.Borders.SolidBorder(1));

        // Add table headers with background color
        var headerCells = new[]
        {
            new Cell().Add(new Paragraph("Item").SimulateBold()),
            new Cell().Add(new Paragraph("Quantity").SimulateBold()),
            new Cell().Add(new Paragraph("Unit Price").SimulateBold()),
            new Cell().Add(new Paragraph("Total Price").SimulateBold())
        };

        foreach (var headerCell in headerCells)
        {
            headerCell.SetBackgroundColor(ColorConstants.LIGHT_GRAY);
            table.AddHeaderCell(headerCell);
        }

        // Add table data
        table.AddCell(order.ItemName);
        table.AddCell(order.Quantity.ToString());
        table.AddCell($"{order.UnitPrice:C}");
        table.AddCell($"{order.TotalPrice:C}");

        // Add the table to the document
        document.Add(table);

        // Add space
        document.Add(new Paragraph("\n"));

        // Add total
        var total = new Paragraph($"Total: {order.TotalPrice:C}")
            .SetTextAlignment(TextAlignment.RIGHT)
            .SimulateBold()
            .SetFontSize(14);
        document.Add(total);

        // Add legal note
        var legalNote = new Paragraph("This is an automatically generated document. " +
                                     "Please contact accounting@easyp2p.com for any questions.")
            .SetFontSize(8)
            .SetTextAlignment(TextAlignment.CENTER);
        document.Add(legalNote);

        // Close the document before accessing the stream
        document.Close();
        pdf.Close();
        writer.Close();

        // Get the PDF bytes
        var pdfBytes = memoryStream.ToArray();
        memoryStream.Close();

        return pdfBytes;
    }
}