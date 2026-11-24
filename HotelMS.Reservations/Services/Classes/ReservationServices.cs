using HotelMS.Reservations.Clients;
using HotelMS.Reservations.DTOs;
using HotelMS.Reservations.Exceptions;
using HotelMS.Reservations.Helpers;
using HotelMS.Reservations.Models;
using HotelMS.Reservations.Models.Enums;
using HotelMS.Reservations.Repositories.Interfaces;
using HotelMS.Reservations.Services.Interfaces;

namespace HotelMS.Reservations.Services.Classes;

public class GuestService : IGuestService
{
    private readonly IGuestRepository _repo;

    public GuestService(IGuestRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<GuestDto>> GetAllAsync()
    {
        var guests = await _repo.GetAllAsync();
        return guests.Select(MapToDto);
    }

    public async Task<GuestDto> GetByIdAsync(int id)
    {
        var guest = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Guest with ID {id} not found.");
        return MapToDto(guest);
    }

    public async Task<GuestDto> CreateAsync(CreateGuestDto dto)
    {
        var existing = await _repo.GetByIdentificationAsync(dto.IdentificationNumber);
        if (existing != null)
            throw new ConflictException($"Guest with identification number '{dto.IdentificationNumber}' already exists.");

        var guest = new Guest
        {
            GuestName = dto.GuestName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            IdentificationNumber = dto.IdentificationNumber
        };

        var created = await _repo.CreateAsync(guest);
        return MapToDto(created);
    }

    public async Task<GuestDto> UpdateAsync(int id, UpdateGuestDto dto)
    {
        var guest = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Guest with ID {id} not found.");

        var existingWithId = await _repo.GetByIdentificationAsync(dto.IdentificationNumber);
        if (existingWithId != null && existingWithId.GuestId != id)
            throw new ConflictException($"Guest with identification number '{dto.IdentificationNumber}' already exists.");

        guest.GuestName = dto.GuestName;
        guest.Email = dto.Email;
        guest.PhoneNumber = dto.PhoneNumber;
        guest.Address = dto.Address;
        guest.IdentificationNumber = dto.IdentificationNumber;

        var updated = await _repo.UpdateAsync(guest);
        return MapToDto(updated);
    }

    private static GuestDto MapToDto(Guest g) => new(
        g.GuestId,
        g.GuestName,
        g.Email,
        g.PhoneNumber,
        g.Address,
        g.IdentificationNumber
    );
}

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepo;
    private readonly IGuestRepository _guestRepo;
    private readonly IHotelClient _hotelClient;
    private readonly IEmailHelper _emailHelper;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(
        IReservationRepository reservationRepo,
        IGuestRepository guestRepo,
        IHotelClient hotelClient,
        IEmailHelper emailHelper,
        ILogger<ReservationService> logger)
    {
        _reservationRepo = reservationRepo;
        _guestRepo = guestRepo;
        _hotelClient = hotelClient;
        _emailHelper = emailHelper;
        _logger = logger;
    }

    public async Task<IEnumerable<ReservationDto>> GetAllAsync()
    {
        var list = await _reservationRepo.GetAllAsync();
        return list.Select(MapToDto);
    }

    public async Task<ReservationDto> GetByIdAsync(int id)
    {
        var r = await _reservationRepo.GetByIdAsync(id) ?? throw new NotFoundException($"Reservation with ID {id} not found.");
        return MapToDto(r);
    }

    public async Task<ReservationDto> GetByCodeAsync(string code)
    {
        var r = await _reservationRepo.GetByCodeAsync(code) ?? throw new NotFoundException($"Reservation with code '{code}' not found.");
        return MapToDto(r);
    }

    public async Task<ReservationDto> CreateAsync(CreateReservationDto dto)
    {
        if (dto.CheckOutDateTime <= dto.CheckInDateTime)
            throw new BadRequestException("CheckOutDateTime must be strictly after CheckInDateTime.");

        var guest = await _guestRepo.GetByIdAsync(dto.GuestId) ?? throw new NotFoundException($"Guest with ID {dto.GuestId} not found.");

        var room = await _hotelClient.GetRoomAsync(dto.RoomId);
        if (room == null || !room.IsActive)
            throw new NotFoundException($"Active room with ID {dto.RoomId} not found.");

        if (room.Capacity < dto.NumberOfGuests)
            throw new BadRequestException($"Room capacity ({room.Capacity}) is insufficient for {dto.NumberOfGuests} guests.");

        // Check date overlap
        var overlaps = await _reservationRepo.GetOverlappingReservationsAsync(dto.RoomId, dto.CheckInDateTime, dto.CheckOutDateTime);
        if (overlaps.Any())
            throw new ConflictException("The selected room is not available for the requested date and time range.");

        // Calculate nightly rates
        decimal totalRoomAmount = 0m;
        var start = DateOnly.FromDateTime(dto.CheckInDateTime.Date);
        var end = DateOnly.FromDateTime(dto.CheckOutDateTime.Date);

        // At least 1 night
        var nights = Math.Max(1, (dto.CheckOutDateTime.Date - dto.CheckInDateTime.Date).Days);
        for (int i = 0; i < nights; i++)
        {
            var nightDate = start.AddDays(i);
            var rate = await _hotelClient.GetEffectiveRateAsync(dto.RoomId, nightDate);
            totalRoomAmount += rate;
        }

        var reservationCode = await _reservationRepo.GenerateReservationCodeAsync();

        var reservation = new Reservation
        {
            ReservationCode = reservationCode,
            GuestId = dto.GuestId,
            RoomId = dto.RoomId,
            CheckInDateTime = dto.CheckInDateTime,
            CheckOutDateTime = dto.CheckOutDateTime,
            NumberOfGuests = dto.NumberOfGuests,
            TotalAmount = totalRoomAmount,
            ReservationStatus = ReservationStatus.PendingPayment,
            CreatedAt = DateTime.UtcNow
        };

        // Handle requested service items
        if (dto.ServiceItems != null && dto.ServiceItems.Any())
        {
            foreach (var svc in dto.ServiceItems)
            {
                var serviceItem = await _hotelClient.GetServiceItemAsync(svc.ServiceId);
                if (serviceItem != null && serviceItem.IsAvailable)
                {
                    var totalPrice = svc.Quantity * serviceItem.Price;
                    reservation.ReservationServiceItems.Add(new ReservationServiceItem
                    {
                        ServiceId = svc.ServiceId,
                        Quantity = svc.Quantity,
                        UnitPrice = serviceItem.Price,
                        TotalPrice = totalPrice
                    });
                    reservation.TotalAmount += totalPrice;
                }
            }
        }

        var created = await _reservationRepo.CreateAsync(reservation);
        created.Guest = guest;

        // Send confirmation email
        _ = Task.Run(() => _emailHelper.SendEmailAsync(
            guest.Email,
            $"Reservation Booking Confirmation - {created.ReservationCode}",
            $"<p>Dear {guest.GuestName},</p><p>Your reservation <strong>{created.ReservationCode}</strong> has been created. Total amount due: ${created.TotalAmount:F2}. Status: {created.ReservationStatus}.</p>"));

        return MapToDto(created);
    }

    public async Task<bool> CancelAsync(int id)
    {
        var reservation = await _reservationRepo.GetByIdAsync(id) ?? throw new NotFoundException($"Reservation with ID {id} not found.");
        if (reservation.ReservationStatus == ReservationStatus.Completed)
            throw new ConflictException("Cannot cancel a completed reservation.");

        reservation.ReservationStatus = ReservationStatus.Cancelled;
        await _reservationRepo.UpdateAsync(reservation);

        if (reservation.Guest != null)
        {
            _ = Task.Run(() => _emailHelper.SendEmailAsync(
                reservation.Guest.Email,
                $"Reservation Cancelled - {reservation.ReservationCode}",
                $"<p>Dear {reservation.Guest.GuestName},</p><p>Your reservation <strong>{reservation.ReservationCode}</strong> has been cancelled.</p>"));
        }

        return true;
    }

    public async Task<ReservationServiceItemDto> AddServiceItemAsync(int reservationId, AddServiceItemDto dto)
    {
        var reservation = await _reservationRepo.GetByIdAsync(reservationId) ?? throw new NotFoundException($"Reservation with ID {reservationId} not found.");
        if (reservation.ReservationStatus == ReservationStatus.Cancelled)
            throw new ConflictException("Cannot add services to a cancelled reservation.");

        var serviceItem = await _hotelClient.GetServiceItemAsync(dto.ServiceId);
        if (serviceItem == null || !serviceItem.IsAvailable)
            throw new NotFoundException($"Active service item with ID {dto.ServiceId} not found.");

        var totalPrice = dto.Quantity * serviceItem.Price;
        var item = new ReservationServiceItem
        {
            ReservationId = reservationId,
            ServiceId = dto.ServiceId,
            Quantity = dto.Quantity,
            UnitPrice = serviceItem.Price,
            TotalPrice = totalPrice
        };

        var created = await _reservationRepo.AddServiceItemAsync(item);

        reservation.TotalAmount += totalPrice;
        await _reservationRepo.UpdateAsync(reservation);

        return new ReservationServiceItemDto(
            created.ReservationServiceItemId,
            created.ReservationId,
            created.ServiceId,
            created.Quantity,
            created.UnitPrice,
            created.TotalPrice
        );
    }

    public async Task<IEnumerable<RoomInfoResponse>> CheckAvailabilityAsync(CheckAvailabilityRequestDto dto)
    {
        if (dto.CheckOutDateTime <= dto.CheckInDateTime)
            throw new BadRequestException("CheckOutDateTime must be strictly after CheckInDateTime.");

        var allRooms = await _hotelClient.GetAllRoomsAsync(activeOnly: true);
        var reservedRoomIds = (await _reservationRepo.GetReservedRoomIdsAsync(dto.CheckInDateTime, dto.CheckOutDateTime)).ToHashSet();

        return allRooms
            .Where(r => !reservedRoomIds.Contains(r.RoomId))
            .Where(r => !dto.Capacity.HasValue || r.Capacity >= dto.Capacity.Value)
            .Where(r => string.IsNullOrEmpty(dto.RoomType) || string.Equals(r.RoomType, dto.RoomType, StringComparison.OrdinalIgnoreCase));
    }

    private static ReservationDto MapToDto(Reservation r) => new(
        r.ReservationId,
        r.ReservationCode,
        r.GuestId,
        r.Guest?.GuestName ?? string.Empty,
        r.RoomId,
        r.CheckInDateTime,
        r.CheckOutDateTime,
        r.NumberOfGuests,
        r.TotalAmount,
        r.ReservationStatus,
        r.CreatedAt,
        r.ReservationServiceItems.Select(s => new ReservationServiceItemDto(
            s.ReservationServiceItemId,
            s.ReservationId,
            s.ServiceId,
            s.Quantity,
            s.UnitPrice,
            s.TotalPrice
        )).ToList()
    );
}

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly IReservationRepository _reservationRepo;
    private readonly IInvoiceRepository _invoiceRepo;
    private readonly IEmailHelper _emailHelper;

    public PaymentService(
        IPaymentRepository paymentRepo,
        IReservationRepository reservationRepo,
        IInvoiceRepository invoiceRepo,
        IEmailHelper emailHelper)
    {
        _paymentRepo = paymentRepo;
        _reservationRepo = reservationRepo;
        _invoiceRepo = invoiceRepo;
        _emailHelper = emailHelper;
    }

    public async Task<IEnumerable<PaymentDto>> GetAllAsync()
    {
        var list = await _paymentRepo.GetAllAsync();
        return list.Select(MapToDto);
    }

    public async Task<PaymentDto> GetByIdAsync(int id)
    {
        var p = await _paymentRepo.GetByIdAsync(id) ?? throw new NotFoundException($"Payment with ID {id} not found.");
        return MapToDto(p);
    }

    public async Task<IEnumerable<PaymentDto>> GetByReservationIdAsync(int reservationId)
    {
        var list = await _paymentRepo.GetByReservationIdAsync(reservationId);
        return list.Select(MapToDto);
    }

    public async Task<PaymentDto> ProcessPaymentAsync(CreatePaymentDto dto)
    {
        var reservation = await _reservationRepo.GetByIdAsync(dto.ReservationId) ?? throw new NotFoundException($"Reservation with ID {dto.ReservationId} not found.");
        if (reservation.ReservationStatus == ReservationStatus.Cancelled)
            throw new ConflictException("Cannot process payment for a cancelled reservation.");

        if (!string.IsNullOrWhiteSpace(dto.TransactionReference))
        {
            var exists = await _paymentRepo.ExistsByTransactionRefAsync(dto.TransactionReference);
            if (exists)
                throw new ConflictException($"Payment with transaction reference '{dto.TransactionReference}' already exists.");
        }

        var payment = new Payment
        {
            ReservationId = dto.ReservationId,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            PaymentStatus = PaymentStatus.Successful,
            TransactionReference = dto.TransactionReference,
            PaymentDate = DateTime.UtcNow
        };

        var created = await _paymentRepo.CreateAsync(payment);

        // Check total paid
        var totalPaid = await _paymentRepo.GetTotalPaidForReservationAsync(dto.ReservationId);
        if (totalPaid >= reservation.TotalAmount && reservation.ReservationStatus == ReservationStatus.PendingPayment)
        {
            reservation.ReservationStatus = ReservationStatus.Confirmed;
            await _reservationRepo.UpdateAsync(reservation);
        }

        // Update invoice if exists
        var invoice = await _invoiceRepo.GetByReservationIdAsync(dto.ReservationId);
        if (invoice != null)
        {
            if (totalPaid >= invoice.TotalAmount)
                invoice.InvoiceStatus = InvoiceStatus.Paid;
            else if (totalPaid > 0)
                invoice.InvoiceStatus = InvoiceStatus.PartiallyPaid;
            await _invoiceRepo.UpdateAsync(invoice);
        }

        if (reservation.Guest != null)
        {
            _ = Task.Run(() => _emailHelper.SendEmailAsync(
                reservation.Guest.Email,
                $"Payment Receipt - {reservation.ReservationCode}",
                $"<p>Dear {reservation.Guest.GuestName},</p><p>We have received your payment of ${payment.Amount:F2} via {payment.PaymentMethod}.</p>"));
        }

        return MapToDto(created);
    }

    private static PaymentDto MapToDto(Payment p) => new(
        p.PaymentId,
        p.ReservationId,
        p.Amount,
        p.PaymentMethod,
        p.PaymentStatus,
        p.TransactionReference,
        p.PaymentDate
    );
}

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepo;
    private readonly IReservationRepository _reservationRepo;
    private readonly IPaymentRepository _paymentRepo;
    private readonly IEmailHelper _emailHelper;

    public InvoiceService(
        IInvoiceRepository invoiceRepo,
        IReservationRepository reservationRepo,
        IPaymentRepository paymentRepo,
        IEmailHelper emailHelper)
    {
        _invoiceRepo = invoiceRepo;
        _reservationRepo = reservationRepo;
        _paymentRepo = paymentRepo;
        _emailHelper = emailHelper;
    }

    public async Task<IEnumerable<InvoiceDto>> GetAllAsync()
    {
        var list = await _invoiceRepo.GetAllAsync();
        return list.Select(MapToDto);
    }

    public async Task<InvoiceDto> GetByIdAsync(int id)
    {
        var i = await _invoiceRepo.GetByIdAsync(id) ?? throw new NotFoundException($"Invoice with ID {id} not found.");
        return MapToDto(i);
    }

    public async Task<InvoiceDto> GetByReservationIdAsync(int reservationId)
    {
        var i = await _invoiceRepo.GetByReservationIdAsync(reservationId) ?? throw new NotFoundException($"Invoice for reservation {reservationId} not found.");
        return MapToDto(i);
    }

    public async Task<InvoiceDto> GenerateInvoiceAsync(int reservationId)
    {
        var existing = await _invoiceRepo.GetByReservationIdAsync(reservationId);
        if (existing != null)
            return MapToDto(existing);

        var reservation = await _reservationRepo.GetByIdAsync(reservationId) ?? throw new NotFoundException($"Reservation with ID {reservationId} not found.");
        if (reservation.ReservationStatus == ReservationStatus.Cancelled)
            throw new ConflictException("Cannot generate invoice for a cancelled reservation.");

        var subTotal = reservation.TotalAmount;
        var taxAmount = Math.Round(subTotal * 0.12m, 2);
        var totalAmount = subTotal + taxAmount;

        var totalPaid = await _paymentRepo.GetTotalPaidForReservationAsync(reservationId);
        InvoiceStatus status;
        if (totalPaid >= totalAmount)
            status = InvoiceStatus.Paid;
        else if (totalPaid > 0)
            status = InvoiceStatus.PartiallyPaid;
        else
            status = InvoiceStatus.Pending;

        var invoice = new Invoice
        {
            ReservationId = reservationId,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{reservationId:D4}",
            IssueDate = DateTime.UtcNow,
            SubTotal = subTotal,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            InvoiceStatus = status
        };

        var created = await _invoiceRepo.CreateAsync(invoice);

        if (reservation.Guest != null)
        {
            _ = Task.Run(() => _emailHelper.SendEmailAsync(
                reservation.Guest.Email,
                $"Invoice Generated - {created.InvoiceNumber}",
                $"<p>Dear {reservation.Guest.GuestName},</p><p>Your invoice <strong>{created.InvoiceNumber}</strong> has been generated for reservation {reservation.ReservationCode}. SubTotal: ${created.SubTotal:F2}, Tax: ${created.TaxAmount:F2}, Total: ${created.TotalAmount:F2}. Status: {created.InvoiceStatus}.</p>"));
        }

        return MapToDto(created);
    }

    private static InvoiceDto MapToDto(Invoice i) => new(
        i.InvoiceId,
        i.ReservationId,
        i.InvoiceNumber,
        i.IssueDate,
        i.SubTotal,
        i.TaxAmount,
        i.TotalAmount,
        i.InvoiceStatus
    );
}
