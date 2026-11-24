using HotelMS.Reservations.Models.Enums;

namespace HotelMS.Reservations.DTOs;

public record GuestDto(
    int GuestId,
    string GuestName,
    string Email,
    string PhoneNumber,
    string? Address,
    string IdentificationNumber
);

public record CreateGuestDto(
    string GuestName,
    string Email,
    string PhoneNumber,
    string? Address,
    string IdentificationNumber
);

public record UpdateGuestDto(
    string GuestName,
    string Email,
    string PhoneNumber,
    string? Address,
    string IdentificationNumber
);

public record ReservationServiceItemDto(
    int ReservationServiceItemId,
    int ReservationId,
    int ServiceId,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);

public record AddServiceItemDto(
    int ServiceId,
    int Quantity
);

public record ReservationDto(
    int ReservationId,
    string ReservationCode,
    int GuestId,
    string GuestName,
    int RoomId,
    DateTime CheckInDateTime,
    DateTime CheckOutDateTime,
    int NumberOfGuests,
    decimal TotalAmount,
    ReservationStatus ReservationStatus,
    DateTime CreatedAt,
    List<ReservationServiceItemDto> ServiceItems
);

public record CreateReservationDto(
    int GuestId,
    int RoomId,
    DateTime CheckInDateTime,
    DateTime CheckOutDateTime,
    int NumberOfGuests,
    List<AddServiceItemDto>? ServiceItems = null
);

public record CheckAvailabilityRequestDto(
    DateTime CheckInDateTime,
    DateTime CheckOutDateTime,
    int? Capacity = null,
    string? RoomType = null
);

public record PaymentDto(
    int PaymentId,
    int ReservationId,
    decimal Amount,
    PaymentMethod PaymentMethod,
    PaymentStatus PaymentStatus,
    string? TransactionReference,
    DateTime PaymentDate
);

public record CreatePaymentDto(
    int ReservationId,
    decimal Amount,
    PaymentMethod PaymentMethod,
    string? TransactionReference = null
);

public record InvoiceDto(
    int InvoiceId,
    int ReservationId,
    string InvoiceNumber,
    DateTime IssueDate,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    InvoiceStatus InvoiceStatus
);
