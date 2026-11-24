using HotelMS.Reservations.Models;

namespace HotelMS.Reservations.Repositories.Interfaces;

public interface IGuestRepository
{
    Task<IEnumerable<Guest>> GetAllAsync();
    Task<Guest?> GetByIdAsync(int id);
    Task<Guest?> GetByIdentificationAsync(string idNumber);
    Task<Guest> CreateAsync(Guest guest);
    Task<Guest> UpdateAsync(Guest guest);
    Task<bool> ExistsAsync(int id);
}

public interface IReservationRepository
{
    Task<IEnumerable<Reservation>> GetAllAsync();
    Task<Reservation?> GetByIdAsync(int id);
    Task<Reservation?> GetByCodeAsync(string code);
    Task<IEnumerable<Reservation>> GetOverlappingReservationsAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null);
    Task<IEnumerable<int>> GetReservedRoomIdsAsync(DateTime checkIn, DateTime checkOut);
    Task<Reservation> CreateAsync(Reservation reservation);
    Task<Reservation> UpdateAsync(Reservation reservation);
    Task<ReservationServiceItem> AddServiceItemAsync(ReservationServiceItem item);
    Task<string> GenerateReservationCodeAsync();
}

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetAllAsync();
    Task<Payment?> GetByIdAsync(int id);
    Task<IEnumerable<Payment>> GetByReservationIdAsync(int reservationId);
    Task<bool> ExistsByTransactionRefAsync(string transactionReference);
    Task<Payment> CreateAsync(Payment payment);
    Task<decimal> GetTotalPaidForReservationAsync(int reservationId);
}

public interface IInvoiceRepository
{
    Task<IEnumerable<Invoice>> GetAllAsync();
    Task<Invoice?> GetByIdAsync(int id);
    Task<Invoice?> GetByReservationIdAsync(int reservationId);
    Task<Invoice> CreateAsync(Invoice invoice);
    Task<Invoice> UpdateAsync(Invoice invoice);
}
