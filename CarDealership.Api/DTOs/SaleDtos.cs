using System.ComponentModel.DataAnnotations;

namespace CarDealership.Api.DTOs;

public record PurchaseRequest(
    [Required] Guid VehicleId
);

public record SaleDto(
    Guid Id,
    Guid VehicleId,
    string VehicleSummary, // e.g. "Toyota Camry 2022"
    decimal PriceAtPurchase,
    DateTime PurchasedAt,
    string Status
);

public record ProcessSaleRequest(
    [Required] Guid SaleId,
    bool Approve
);
