using HotelMS.Reservations.Data;
using HotelMS.Reservations.Models;
using HotelMS.Reservations.Models.Enums;
using HotelMS.Reservations.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelMS.Reservations.Repositories.Classes;

public class GuestRepository : IGuestRepository
{
    private readonly ReservationDbContext _context;

    public GuestRepository(ReservationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Guest>> GetAllAsync()
    {
        return await _context.Guests.AsNoTracking().ToListAsync();
    }

    public async Task<Guest?> GetByIdAsync(int id)
    {
        return await _context.Guests.FindAsync(id);
    }

    public async Task<Guest?> GetByIdentificationAsync(string idNumber)
    {
        return await _context.Guests.FirstOrDefaultAsync(g => g.IdentificationNumber == idNumber);
    }

    public async Task<Guest> CreateAsync(Guest guest)
    {
        _context.Guests.Add(guest);
        await _context.SaveChangesAsync();
        return guest;
    }

    public async Task<Guest> UpdateAsync(Guest guest)
    {
        _context.Guests.Update(guest);
        await _context.SaveChangesAsync();
        return guest;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Guests.AnyAsync(g => g.GuestId == id);
    }
}

public class ReservationRepository : IReservationRepository
{
    private readonly ReservationDbContext _context;

    public ReservationRepository(ReservationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Reservation>> GetAllAsync()
    {
        return await _context.Reservations
            .Include(r => r.Guest)
            .Include(r => r.ReservationServiceItems)
            .Include(r => r.Payments)
            .Include(r => r.Invoice)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Reservation?> GetByIdAsync(int id)
    {
        return await _context.Reservations
            .Include(r => r.Guest)
            .Include(r => r.ReservationServiceItems)
            .Include(r => r.Payments)
            .Include(r => r.Invoice)
            .FirstOrDefaultAsync(r => r.ReservationId == id);
    }

    public async Task<Reservation?> GetByCodeAsync(string code)
    {
        return await _context.Reservations
            .Include(r => r.Guest)
            .Include(r => r.ReservationServiceItems)
            .Include(r => r.Payments)
            .Include(r => r.Invoice)
            .FirstOrDefaultAsync(r => r.ReservationCode == code);
    }

    public async Task<IEnumerable<Reservation>> GetOverlappingReservationsAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null)
    {
        return await _context.Reservations
            .Where(r => r.RoomId == roomId &&
                        r.ReservationStatus != ReservationStatus.Cancelled &&
                        (excludeReservationId == null || r.ReservationId != excludeReservationId.Value) &&
                        checkIn < r.CheckOutDateTime && checkOut > r.CheckInDateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<int>> GetReservedRoomIdsAsync(DateTime checkIn, DateTime checkOut)
    {
        return await _context.Reservations
            .Where(r => r.ReservationStatus != ReservationStatus.Cancelled &&
                        checkIn < r.CheckOutDateTime && checkOut > r.CheckInDateTime)
            .Select(r => r.RoomId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<Reservation> CreateAsync(Reservation reservation)
    {
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();
        return reservation;
    }

    public async Task<Reservation> UpdateAsync(Reservation reservation)
    {
        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync();
        return reservation;
    }

    public async Task<ReservationServiceItem> AddServiceItemAsync(ReservationServiceItem item)
    {
        _context.ReservationServiceItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<string> GenerateReservationCodeAsync()
    {
        if (_context.Database.IsSqlServer())
        {
            try
            {
                // SQL Server sequence
                var connection = _context.Database.GetDbConnection();
                await using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT NEXT VALUE FOR ReservationCodeSeq";
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                if (result != null)
                {
                    return $"RES{result}";
                }
            }
            catch
            {
                // Fallback to calculation
            }
        }

        var maxId = await _context.Reservations.AnyAsync() 
            ? await _context.Reservations.MaxAsync(r => r.ReservationId) 
            : 0;
        return $"RES{1001 + maxId}";
    }
}

public class PaymentRepository : IPaymentRepository
{
    private readonly ReservationDbContext _context;

    public PaymentRepository(ReservationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Payment>> GetAllAsync()
    {
        return await _context.Payments.AsNoTracking().ToListAsync();
    }

    public async Task<Payment?> GetByIdAsync(int id)
    {
        return await _context.Payments.FindAsync(id);
    }

    public async Task<IEnumerable<Payment>> GetByReservationIdAsync(int reservationId)
    {
        return await _context.Payments
            .Where(p => p.ReservationId == reservationId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> ExistsByTransactionRefAsync(string transactionReference)
    {
        if (string.IsNullOrWhiteSpace(transactionReference)) return false;
        return await _context.Payments.AnyAsync(p => p.TransactionReference == transactionReference);
    }

    public async Task<Payment> CreateAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<decimal> GetTotalPaidForReservationAsync(int reservationId)
    {
        return await _context.Payments
            .Where(p => p.ReservationId == reservationId && p.PaymentStatus == PaymentStatus.Successful)
            .SumAsync(p => p.Amount);
    }
}

public class InvoiceRepository : IInvoiceRepository
{
    private readonly ReservationDbContext _context;

    public InvoiceRepository(ReservationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync()
    {
        return await _context.Invoices.AsNoTracking().ToListAsync();
    }

    public async Task<Invoice?> GetByIdAsync(int id)
    {
        return await _context.Invoices.FindAsync(id);
    }

    public async Task<Invoice?> GetByReservationIdAsync(int reservationId)
    {
        return await _context.Invoices.FirstOrDefaultAsync(i => i.ReservationId == reservationId);
    }

    public async Task<Invoice> CreateAsync(Invoice invoice)
    {
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task<Invoice> UpdateAsync(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }
}
