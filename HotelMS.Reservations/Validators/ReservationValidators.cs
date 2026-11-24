using FluentValidation;
using HotelMS.Reservations.DTOs;

namespace HotelMS.Reservations.Validators;

public class CreateGuestValidator : AbstractValidator<CreateGuestDto>
{
    public CreateGuestValidator()
    {
        RuleFor(x => x.GuestName).NotEmpty().WithMessage("Guest name is required.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Valid email is required.");
        RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required.");
        RuleFor(x => x.IdentificationNumber).NotEmpty().WithMessage("Identification number is required.");
    }
}

public class CreateReservationValidator : AbstractValidator<CreateReservationDto>
{
    public CreateReservationValidator()
    {
        RuleFor(x => x.GuestId).GreaterThan(0).WithMessage("GuestId must be valid.");
        RuleFor(x => x.RoomId).GreaterThan(0).WithMessage("RoomId must be valid.");
        RuleFor(x => x.NumberOfGuests).GreaterThan(0).WithMessage("NumberOfGuests must be at least 1.");
        RuleFor(x => x.CheckOutDateTime).GreaterThan(x => x.CheckInDateTime)
            .WithMessage("CheckOutDateTime must be after CheckInDateTime.");
    }
}

public class CreatePaymentValidator : AbstractValidator<CreatePaymentDto>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.ReservationId).GreaterThan(0).WithMessage("ReservationId must be valid.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Payment amount must be greater than zero.");
        RuleFor(x => x.PaymentMethod).IsInEnum().WithMessage("Invalid payment method.");
    }
}

public class AddServiceItemValidator : AbstractValidator<AddServiceItemDto>
{
    public AddServiceItemValidator()
    {
        RuleFor(x => x.ServiceId).GreaterThan(0).WithMessage("ServiceId must be valid.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be at least 1.");
    }
}
