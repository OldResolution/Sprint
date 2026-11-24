using HotelMS.Reservations.Models.Enums;

namespace HotelMS.Reservations.Models;

public class Guest
{
    public int GuestId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string IdentificationNumber { get; set; } = string.Empty;

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}

public class Reservation
{
    public int ReservationId { get; set; }
    public string ReservationCode { get; set; } = string.Empty;
    public int GuestId { get; set; }
    public Guest Guest { get; set; } = null!;
    
    // Logical cross-service reference to HotelDB Room
    public int RoomId { get; set; }
    
    public DateTime CheckInDateTime { get; set; }
    public DateTime CheckOutDateTime { get; set; }
    public int NumberOfGuests { get; set; }
    public decimal TotalAmount { get; set; }
    public ReservationStatus ReservationStatus { get; set; } = ReservationStatus.PendingPayment;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ReservationServiceItem> ReservationServiceItems { get; set; } = new List<ReservationServiceItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public Invoice? Invoice { get; set; }
}

public class ReservationServiceItem
{
    public int ReservationServiceItemId { get; set; }
    public int ReservationId { get; set; }
    public Reservation Reservation { get; set; } = null!;

    // Logical cross-service reference to HotelDB RoomServiceItem
    public int ServiceId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class Payment
{
    public int PaymentId { get; set; }
    public int ReservationId { get; set; }
    public Reservation Reservation { get; set; } = null!;

    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public string? TransactionReference { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
}

public class Invoice
{
    public int InvoiceId { get; set; }
    public int ReservationId { get; set; }
    public Reservation Reservation { get; set; } = null!;

    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; } // 12%
    public decimal TotalAmount { get; set; }
    public InvoiceStatus InvoiceStatus { get; set; } = InvoiceStatus.Pending;
}
