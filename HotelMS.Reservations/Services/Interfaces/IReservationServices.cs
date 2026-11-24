using HotelMS.Reservations.Clients;
using HotelMS.Reservations.DTOs;

namespace HotelMS.Reservations.Services.Interfaces;

public interface IGuestService
{
    Task<IEnumerable<GuestDto>> GetAllAsync();
    Task<GuestDto> GetByIdAsync(int id);
    Task<GuestDto> CreateAsync(CreateGuestDto dto);
    Task<GuestDto> UpdateAsync(int id, UpdateGuestDto dto);
}

public interface IReservationService
{
    Task<IEnumerable<ReservationDto>> GetAllAsync();
    Task<ReservationDto> GetByIdAsync(int id);
    Task<ReservationDto> GetByCodeAsync(string code);
    Task<ReservationDto> CreateAsync(CreateReservationDto dto);
    Task<bool> CancelAsync(int id);
    Task<ReservationServiceItemDto> AddServiceItemAsync(int reservationId, AddServiceItemDto dto);
    Task<IEnumerable<RoomInfoResponse>> CheckAvailabilityAsync(CheckAvailabilityRequestDto dto);
}

public interface IPaymentService
{
    Task<IEnumerable<PaymentDto>> GetAllAsync();
    Task<PaymentDto> GetByIdAsync(int id);
    Task<IEnumerable<PaymentDto>> GetByReservationIdAsync(int reservationId);
    Task<PaymentDto> ProcessPaymentAsync(CreatePaymentDto dto);
}

public interface IInvoiceService
{
    Task<IEnumerable<InvoiceDto>> GetAllAsync();
    Task<InvoiceDto> GetByIdAsync(int id);
    Task<InvoiceDto> GetByReservationIdAsync(int reservationId);
    Task<InvoiceDto> GenerateInvoiceAsync(int reservationId);
}
