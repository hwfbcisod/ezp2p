// Extensions/SupplierMappingExtensions.cs
using EasyP2P.Web.Models;
using EasyP2P.Web.Models.Database;

namespace EasyP2P.Web.Extensions;

public static class SupplierMappingExtensions
{
    public static SupplierViewModel ToViewModel(this SupplierDatabaseModel dbModel)
    {
        return new SupplierViewModel
        {
            Id = dbModel.Id,
            Name = dbModel.Name,
            ContactPerson = dbModel.ContactPerson,
            Email = dbModel.Email,
            Phone = dbModel.Phone,
            Address = dbModel.Address,
            City = dbModel.City,
            State = dbModel.State,
            Country = dbModel.Country,
            PostalCode = dbModel.PostalCode,
            TaxId = dbModel.TaxId,
            PaymentTerms = dbModel.PaymentTerms,
            Status = dbModel.Status,
            Rating = dbModel.Rating,
            Website = dbModel.Website,
            Notes = dbModel.Notes,
            CreatedDate = dbModel.CreatedDate,
            CreatedBy = dbModel.CreatedBy,
            LastUpdated = dbModel.LastUpdated,
            UpdatedBy = dbModel.UpdatedBy
        };
    }

    public static IEnumerable<SupplierViewModel> ToViewModels(this IEnumerable<SupplierDatabaseModel> dbModels)
    {
        return dbModels.Select(ToViewModel);
    }

    public static SupplierInputModel ToInputModel(this SupplierDatabaseModel dbModel)
    {
        return new SupplierInputModel
        {
            Name = dbModel.Name,
            ContactPerson = dbModel.ContactPerson,
            Email = dbModel.Email,
            Phone = dbModel.Phone,
            Address = dbModel.Address,
            City = dbModel.City,
            State = dbModel.State,
            Country = dbModel.Country,
            PostalCode = dbModel.PostalCode,
            TaxId = dbModel.TaxId,
            PaymentTerms = dbModel.PaymentTerms,
            Status = dbModel.Status,
            Rating = dbModel.Rating,
            Website = dbModel.Website,
            Notes = dbModel.Notes
        };
    }

    public static SupplierInputModel ToInputModel(this SupplierViewModel viewModel)
    {
        return new SupplierInputModel
        {
            Name = viewModel.Name,
            ContactPerson = viewModel.ContactPerson,
            Email = viewModel.Email,
            Phone = viewModel.Phone,
            Address = viewModel.Address,
            City = viewModel.City,
            State = viewModel.State,
            Country = viewModel.Country,
            PostalCode = viewModel.PostalCode,
            TaxId = viewModel.TaxId,
            PaymentTerms = viewModel.PaymentTerms,
            Status = viewModel.Status,
            Rating = viewModel.Rating,
            Website = viewModel.Website,
            Notes = viewModel.Notes
        };
    }
}