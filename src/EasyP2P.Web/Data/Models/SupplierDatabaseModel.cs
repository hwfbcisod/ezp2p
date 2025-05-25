namespace EasyP2P.Web.Models.Database;

/// <summary>
/// Database model representing a supplier in the database.
/// Maps directly to the suppliers table.
/// </summary>
public class SupplierDatabaseModel
{
    /// <summary>
    /// Primary key identifier for the supplier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the supplier company.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Primary contact person at the supplier.
    /// </summary>
    public string ContactPerson { get; set; } = "";

    /// <summary>
    /// Contact email address.
    /// </summary>
    public string Email { get; set; } = "";

    /// <summary>
    /// Contact phone number.
    /// </summary>
    public string Phone { get; set; } = "";

    /// <summary>
    /// Street address.
    /// </summary>
    public string Address { get; set; } = "";

    /// <summary>
    /// City.
    /// </summary>
    public string City { get; set; } = "";

    /// <summary>
    /// State or province.
    /// </summary>
    public string State { get; set; } = "";

    /// <summary>
    /// Country.
    /// </summary>
    public string Country { get; set; } = "";

    /// <summary>
    /// Postal or ZIP code.
    /// </summary>
    public string PostalCode { get; set; } = "";

    /// <summary>
    /// Tax identification number.
    /// </summary>
    public string TaxId { get; set; } = "";

    /// <summary>
    /// Payment terms (e.g., Net 30, Net 15).
    /// </summary>
    public string PaymentTerms { get; set; } = "";

    /// <summary>
    /// Current status of the supplier.
    /// </summary>
    public string Status { get; set; } = "";

    /// <summary>
    /// Supplier rating (1-5 stars).
    /// </summary>
    public int? Rating { get; set; }

    /// <summary>
    /// Supplier website URL.
    /// </summary>
    public string Website { get; set; } = "";

    /// <summary>
    /// Additional notes about the supplier.
    /// </summary>
    public string Notes { get; set; } = "";

    /// <summary>
    /// Date when the supplier was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// User who created the supplier record.
    /// </summary>
    public string CreatedBy { get; set; } = "";

    /// <summary>
    /// Date when the supplier was last updated.
    /// </summary>
    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// User who last updated the supplier record.
    /// </summary>
    public string UpdatedBy { get; set; } = "";
}