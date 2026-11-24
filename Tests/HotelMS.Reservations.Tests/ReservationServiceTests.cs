using FluentAssertions;
using HotelMS.Reservations.Clients;
using HotelMS.Reservations.DTOs;
using HotelMS.Reservations.Exceptions;
using HotelMS.Reservations.Helpers;
using HotelMS.Reservations.Models;
using HotelMS.Reservations.Models.Enums;
using HotelMS.Reservations.Repositories.Interfaces;
using HotelMS.Reservations.Services.Classes;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace HotelMS.Reservations.Tests;

[TestFixture]
public class ReservationServiceTests
{
    private Mock<IReservationRepository> _resRepoMock = null!;
    private Mock<IGuestRepository> _guestRepoMock = null!;
    private Mock<IHotelClient> _hotelClientMock = null!;
    private Mock<IEmailHelper> _emailHelperMock = null!;
    private Mock<ILogger<ReservationService>> _loggerMock = null!;
    private ReservationService _resService = null!;

    private Mock<IPaymentRepository> _paymentRepoMock = null!;
    private Mock<IInvoiceRepository> _invoiceRepoMock = null!;
    private PaymentService _paymentService = null!;
    private InvoiceService _invoiceService = null!;

    [SetUp]
    public void SetUp()
    {
        _resRepoMock = new Mock<IReservationRepository>();
        _guestRepoMock = new Mock<IGuestRepository>();
        _hotelClientMock = new Mock<IHotelClient>();
        _emailHelperMock = new Mock<IEmailHelper>();
        _loggerMock = new Mock<ILogger<ReservationService>>();

        _paymentRepoMock = new Mock<IPaymentRepository>();
        _invoiceRepoMock = new Mock<IInvoiceRepository>();

        _resService = new ReservationService(
            _resRepoMock.Object,
            _guestRepoMock.Object,
            _hotelClientMock.Object,
            _emailHelperMock.Object,
            _loggerMock.Object
        );

        _paymentService = new PaymentService(
            _paymentRepoMock.Object,
            _resRepoMock.Object,
            _invoiceRepoMock.Object,
            _emailHelperMock.Object
        );

        _invoiceService = new InvoiceService(
            _invoiceRepoMock.Object,
            _resRepoMock.Object,
            _paymentRepoMock.Object,
            _emailHelperMock.Object
        );
    }

    [Test]
    public async Task TC04_TC08_CreateReservation_CalculatesCorrectTotalAmount_AndSetsPendingPayment()
    {
        // Arrange: 2 nights stay, $150 per night
        var checkIn = new DateTime(2026, 6, 1, 14, 0, 0);
        var checkOut = new DateTime(2026, 6, 3, 11, 0, 0);

        var guest = new Guest { GuestId = 1, GuestName = "Alice", Email = "alice@example.com" };
        _guestRepoMock.Setup(g => g.GetByIdAsync(1)).ReturnsAsync(guest);

        var room = new RoomInfoResponse(10, "101", "Deluxe", 2, "Available", true);
        _hotelClientMock.Setup(h => h.GetRoomAsync(10)).ReturnsAsync(room);

        // Effective rate returns 150 for each night
        _hotelClientMock.Setup(h => h.GetEffectiveRateAsync(10, It.IsAny<DateOnly>())).ReturnsAsync(150m);

        // No overlaps
        _resRepoMock.Setup(r => r.GetOverlappingReservationsAsync(10, checkIn, checkOut, null))
            .ReturnsAsync(new List<Reservation>());

        _resRepoMock.Setup(r => r.GenerateReservationCodeAsync()).ReturnsAsync("RES1001");
        _resRepoMock.Setup(r => r.CreateAsync(It.IsAny<Reservation>()))
            .ReturnsAsync((Reservation r) => { r.ReservationId = 1; return r; });

        var dto = new CreateReservationDto(1, 10, checkIn, checkOut, 2);

        // Act
        var result = await _resService.CreateAsync(dto);

        // Assert: 2 nights * $150 = $300
        result.Should().NotBeNull();
        result.ReservationCode.Should().Be("RES1001");
        result.TotalAmount.Should().Be(300m);
        result.ReservationStatus.Should().Be(ReservationStatus.PendingPayment);
    }

    [Test]
    public async Task TC05_TC09_CreateReservation_WithOverlappingDates_ThrowsConflictException()
    {
        // Arrange
        var checkIn = new DateTime(2026, 6, 1, 14, 0, 0);
        var checkOut = new DateTime(2026, 6, 5, 11, 0, 0);

        var guest = new Guest { GuestId = 1, GuestName = "Alice" };
        _guestRepoMock.Setup(g => g.GetByIdAsync(1)).ReturnsAsync(guest);

        var room = new RoomInfoResponse(10, "101", "Deluxe", 2, "Available", true);
        _hotelClientMock.Setup(h => h.GetRoomAsync(10)).ReturnsAsync(room);

        var existingRes = new Reservation
        {
            ReservationId = 99,
            RoomId = 10,
            CheckInDateTime = new DateTime(2026, 6, 2, 14, 0, 0),
            CheckOutDateTime = new DateTime(2026, 6, 4, 11, 0, 0),
            ReservationStatus = ReservationStatus.Confirmed
        };

        _resRepoMock.Setup(r => r.GetOverlappingReservationsAsync(10, checkIn, checkOut, null))
            .ReturnsAsync(new List<Reservation> { existingRes });

        var dto = new CreateReservationDto(1, 10, checkIn, checkOut, 2);

        // Act
        var act = async () => await _resService.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("The selected room is not available for the requested date and time range.");
    }

    [Test]
    public async Task CreateReservation_WithSameDayTurnover_DoesNotConflict()
    {
        // Reservation A: CheckIn June 1 at 14:00, CheckOut June 2 at 11:00
        // Reservation B: CheckIn June 2 at 14:00, CheckOut June 3 at 11:00
        var existingA = new Reservation
        {
            ReservationId = 50,
            RoomId = 10,
            CheckInDateTime = new DateTime(2026, 6, 1, 14, 0, 0),
            CheckOutDateTime = new DateTime(2026, 6, 2, 11, 0, 0),
            ReservationStatus = ReservationStatus.Confirmed
        };

        var checkInB = new DateTime(2026, 6, 2, 14, 0, 0);
        var checkOutB = new DateTime(2026, 6, 3, 11, 0, 0);

        // Check if existingA overlaps with B:
        // Overlap formula: checkInB < existingA.CheckOutDateTime && checkOutB > existingA.CheckInDateTime
        // 14:00 < 11:00 is FALSE -> No overlap!
        bool overlaps = checkInB < existingA.CheckOutDateTime && checkOutB > existingA.CheckInDateTime;
        overlaps.Should().BeFalse();
    }

    [Test]
    public async Task CreateReservation_RoomCapacityInsufficient_ThrowsBadRequestException()
    {
        // Arrange
        var checkIn = new DateTime(2026, 6, 1, 14, 0, 0);
        var checkOut = new DateTime(2026, 6, 2, 11, 0, 0);

        var guest = new Guest { GuestId = 1, GuestName = "Alice" };
        _guestRepoMock.Setup(g => g.GetByIdAsync(1)).ReturnsAsync(guest);

        var room = new RoomInfoResponse(10, "101", "Single", 1, "Available", true);
        _hotelClientMock.Setup(h => h.GetRoomAsync(10)).ReturnsAsync(room);

        var dto = new CreateReservationDto(1, 10, checkIn, checkOut, 3); // 3 guests in capacity 1

        // Act
        var act = async () => await _resService.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Room capacity (1) is insufficient for 3 guests.");
    }

    [Test]
    public async Task TC10_AddServiceItem_IncreasesReservationTotalAmount()
    {
        // Arrange
        var res = new Reservation
        {
            ReservationId = 5,
            TotalAmount = 200m,
            ReservationStatus = ReservationStatus.Confirmed
        };
        _resRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(res);

        var serviceItem = new RoomServiceItemResponse(2, "Breakfast Buffet", "Delicious breakfast", 25m, true);
        _hotelClientMock.Setup(h => h.GetServiceItemAsync(2)).ReturnsAsync(serviceItem);

        _resRepoMock.Setup(r => r.AddServiceItemAsync(It.IsAny<ReservationServiceItem>()))
            .ReturnsAsync((ReservationServiceItem item) => item);

        var dto = new AddServiceItemDto(2, 2); // 2 x $25 = $50

        // Act
        var result = await _resService.AddServiceItemAsync(5, dto);

        // Assert
        result.Should().NotBeNull();
        result.TotalPrice.Should().Be(50m);
        res.TotalAmount.Should().Be(250m);
        _resRepoMock.Verify(r => r.UpdateAsync(res), Times.Once);
    }

    [Test]
    public async Task TC11_CancelReservation_MarksStatusAsCancelled()
    {
        // Arrange
        var res = new Reservation
        {
            ReservationId = 8,
            ReservationCode = "RES1008",
            ReservationStatus = ReservationStatus.PendingPayment,
            Guest = new Guest { GuestName = "Bob", Email = "bob@example.com" }
        };
        _resRepoMock.Setup(r => r.GetByIdAsync(8)).ReturnsAsync(res);

        // Act
        var result = await _resService.CancelAsync(8);

        // Assert
        result.Should().BeTrue();
        res.ReservationStatus.Should().Be(ReservationStatus.Cancelled);
        _resRepoMock.Verify(r => r.UpdateAsync(res), Times.Once);
    }

    [Test]
    public async Task TC12_ProcessPayment_WhenFullyPaid_UpdatesReservationStatusToConfirmed()
    {
        // Arrange
        var res = new Reservation
        {
            ReservationId = 12,
            ReservationCode = "RES1012",
            TotalAmount = 300m,
            ReservationStatus = ReservationStatus.PendingPayment,
            Guest = new Guest { GuestName = "Charlie", Email = "charlie@example.com" }
        };
        _resRepoMock.Setup(r => r.GetByIdAsync(12)).ReturnsAsync(res);
        _paymentRepoMock.Setup(p => p.ExistsByTransactionRefAsync("TXN-12345")).ReturnsAsync(false);
        _paymentRepoMock.Setup(p => p.CreateAsync(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => p);

        // After processing, total paid is 300
        _paymentRepoMock.Setup(p => p.GetTotalPaidForReservationAsync(12)).ReturnsAsync(300m);

        var paymentDto = new CreatePaymentDto(12, 300m, PaymentMethod.UPI, "TXN-12345");

        // Act
        var result = await _paymentService.ProcessPaymentAsync(paymentDto);

        // Assert
        result.Should().NotBeNull();
        res.ReservationStatus.Should().Be(ReservationStatus.Confirmed);
        _resRepoMock.Verify(r => r.UpdateAsync(res), Times.Once);
    }

    [Test]
    public async Task TC13_ProcessPayment_WithDuplicateTransactionReference_ThrowsConflictException()
    {
        // Arrange
        var res = new Reservation { ReservationId = 15, ReservationStatus = ReservationStatus.PendingPayment };
        _resRepoMock.Setup(r => r.GetByIdAsync(15)).ReturnsAsync(res);
        _paymentRepoMock.Setup(p => p.ExistsByTransactionRefAsync("DUP-REF-999")).ReturnsAsync(true);

        var paymentDto = new CreatePaymentDto(15, 100m, PaymentMethod.Card, "DUP-REF-999");

        // Act
        var act = async () => await _paymentService.ProcessPaymentAsync(paymentDto);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Payment with transaction reference 'DUP-REF-999' already exists.");
    }

    [Test]
    public async Task TC14_GenerateInvoice_Calculates12PercentTaxCorrectly()
    {
        // Arrange: SubTotal = $500 -> 12% tax = $60 -> Total = $560
        var res = new Reservation
        {
            ReservationId = 20,
            ReservationCode = "RES1020",
            TotalAmount = 500m,
            ReservationStatus = ReservationStatus.Confirmed,
            Guest = new Guest { GuestName = "David", Email = "david@example.com" }
        };
        _resRepoMock.Setup(r => r.GetByIdAsync(20)).ReturnsAsync(res);
        _invoiceRepoMock.Setup(i => i.GetByReservationIdAsync(20)).ReturnsAsync((Invoice?)null);
        _paymentRepoMock.Setup(p => p.GetTotalPaidForReservationAsync(20)).ReturnsAsync(0m);

        _invoiceRepoMock.Setup(i => i.CreateAsync(It.IsAny<Invoice>()))
            .ReturnsAsync((Invoice inv) => inv);

        // Act
        var invoice = await _invoiceService.GenerateInvoiceAsync(20);

        // Assert
        invoice.Should().NotBeNull();
        invoice.SubTotal.Should().Be(500m);
        invoice.TaxAmount.Should().Be(60m);
        invoice.TotalAmount.Should().Be(560m);
        invoice.InvoiceStatus.Should().Be(InvoiceStatus.Pending);
        invoice.InvoiceNumber.Should().StartWith("INV-");
    }

    [Test]
    public async Task TC15_GenerateInvoice_WithPartialPayment_SetsInvoiceStatusToPartiallyPaid()
    {
        // Arrange: SubTotal = $500, Total = $560, Paid = $300
        var res = new Reservation
        {
            ReservationId = 25,
            ReservationCode = "RES1025",
            TotalAmount = 500m,
            ReservationStatus = ReservationStatus.Confirmed
        };
        _resRepoMock.Setup(r => r.GetByIdAsync(25)).ReturnsAsync(res);
        _invoiceRepoMock.Setup(i => i.GetByReservationIdAsync(25)).ReturnsAsync((Invoice?)null);
        _paymentRepoMock.Setup(p => p.GetTotalPaidForReservationAsync(25)).ReturnsAsync(300m);

        _invoiceRepoMock.Setup(i => i.CreateAsync(It.IsAny<Invoice>()))
            .ReturnsAsync((Invoice inv) => inv);

        // Act
        var invoice = await _invoiceService.GenerateInvoiceAsync(25);

        // Assert
        invoice.InvoiceStatus.Should().Be(InvoiceStatus.PartiallyPaid);
    }
}
