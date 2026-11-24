using FluentValidation;
using HotelMS.HotelManagement.DTOs;
using HotelMS.HotelManagement.Models.Enums;

namespace HotelMS.HotelManagement.Validators;

public class CreateRoomValidator : AbstractValidator<CreateRoomDto>
{
    public CreateRoomValidator()
    {
        RuleFor(x => x.RoomNumber).NotEmpty().WithMessage("Room number is required.");
        RuleFor(x => x.Capacity).GreaterThan(0).WithMessage("Capacity must be greater than zero.");
        RuleFor(x => x.RoomType).IsEnumName(typeof(RoomType), caseSensitive: false)
            .WithMessage("RoomType must be Single, Double, Deluxe, Suite, or Hall.");
        RuleFor(x => x.Status).IsEnumName(typeof(RoomStatus), caseSensitive: false)
            .WithMessage("Status must be Available, Occupied, or Maintenance.");
    }
}

public class CreateRoomRateValidator : AbstractValidator<CreateRoomRateDto>
{
    public CreateRoomRateValidator()
    {
        RuleFor(x => x.RoomId).GreaterThan(0).WithMessage("Valid RoomId is required.");
        RuleFor(x => x.BaseRate).GreaterThan(0).WithMessage("Base rate must be greater than zero.");
        RuleFor(x => x.SurgeRate).GreaterThanOrEqualTo(0).WithMessage("Surge rate cannot be negative.");
        When(x => x.EffectiveTo.HasValue, () =>
        {
            RuleFor(x => x.EffectiveTo!.Value).GreaterThanOrEqualTo(x => x.EffectiveFrom)
                .WithMessage("EffectiveTo date must be on or after EffectiveFrom date.");
        });
    }
}

public class CreateRoomServiceItemValidator : AbstractValidator<CreateRoomServiceItemDto>
{
    public CreateRoomServiceItemValidator()
    {
        RuleFor(x => x.ServiceName).NotEmpty().WithMessage("Service name is required.");
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");
    }
}

public class CreateInventoryItemValidator : AbstractValidator<CreateInventoryItemDto>
{
    public CreateInventoryItemValidator()
    {
        RuleFor(x => x.ItemName).NotEmpty().WithMessage("Item name is required.");
        RuleFor(x => x.Category).NotEmpty().WithMessage("Category is required.");
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.");
        RuleFor(x => x.ReorderLevel).GreaterThanOrEqualTo(0).WithMessage("Reorder level cannot be negative.");
    }
}
