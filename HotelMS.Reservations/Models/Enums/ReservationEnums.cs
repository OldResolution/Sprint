namespace HotelMS.Reservations.Models.Enums;

public enum ReservationStatus
{
    PendingPayment,
    Confirmed,
    Cancelled,
    Completed
}

public enum PaymentStatus
{
    Pending,
    Successful,
    Failed
}

public enum InvoiceStatus
{
    Pending,
    PartiallyPaid,
    Paid
}

public enum PaymentMethod
{
    Card,
    UPI,
    Cash
}
