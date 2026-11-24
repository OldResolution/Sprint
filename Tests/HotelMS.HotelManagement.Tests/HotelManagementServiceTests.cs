using FluentAssertions;
using HotelMS.HotelManagement.DTOs;
using HotelMS.HotelManagement.Exceptions;
using HotelMS.HotelManagement.Models;
using HotelMS.HotelManagement.Models.Enums;
using HotelMS.HotelManagement.Repositories.Interfaces;
using HotelMS.HotelManagement.Services.Classes;
using Moq;
using NUnit.Framework;

namespace HotelMS.HotelManagement.Tests;

[TestFixture]
public class HotelManagementServiceTests
{
    private Mock<IRoomRepository> _roomRepoMock = null!;
    private Mock<IRateRepository> _rateRepoMock = null!;
    private Mock<IServiceCatalogRepository> _catalogRepoMock = null!;
    private Mock<IInventoryRepository> _inventoryRepoMock = null!;

    private RoomService _roomService = null!;
    private RateService _rateService = null!;
    private ServiceCatalogService _catalogService = null!;

    [SetUp]
    public void SetUp()
    {
        _roomRepoMock = new Mock<IRoomRepository>();
        _rateRepoMock = new Mock<IRateRepository>();
        _catalogRepoMock = new Mock<IServiceCatalogRepository>();
        _inventoryRepoMock = new Mock<IInventoryRepository>();

        _roomService = new RoomService(_roomRepoMock.Object);
        _rateService = new RateService(_rateRepoMock.Object, _roomRepoMock.Object);
        _catalogService = new ServiceCatalogService(_catalogRepoMock.Object);
    }

    [Test]
    public async Task CreateRoom_WithUniqueNumber_Succeeds()
    {
        // Arrange
        _roomRepoMock.Setup(r => r.ExistsByNumberAsync("101", null))
            .ReturnsAsync(false);
        _roomRepoMock.Setup(r => r.CreateAsync(It.IsAny<Room>()))
            .ReturnsAsync((Room r) => { r.RoomId = 1; return r; });

        var dto = new CreateRoomDto
        {
            RoomNumber = "101",
            RoomType = "Deluxe",
            Capacity = 2,
            Status = "Available"
        };

        // Act
        var result = await _roomService.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.RoomNumber.Should().Be("101");
        result.RoomType.Should().Be("Deluxe");
        result.Capacity.Should().Be(2);
        result.IsActive.Should().BeTrue();
    }

    [Test]
    public async Task CreateRoom_WithDuplicateNumber_ThrowsConflictException()
    {
        // Arrange
        _roomRepoMock.Setup(r => r.ExistsByNumberAsync("101", null))
            .ReturnsAsync(true);

        var dto = new CreateRoomDto
        {
            RoomNumber = "101",
            RoomType = "Deluxe",
            Capacity = 2,
            Status = "Available"
        };

        // Act
        var act = async () => await _roomService.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Room number '101' already exists.");
    }

    [Test]
    public async Task DeleteRoom_PerformsSoftDelete()
    {
        // Arrange
        _roomRepoMock.Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(new Room { RoomId = 5, RoomNumber = "105" });
        _roomRepoMock.Setup(r => r.SoftDeleteAsync(5))
            .ReturnsAsync(true);

        // Act
        var result = await _roomService.DeleteAsync(5);

        // Assert
        result.Should().BeTrue();
        _roomRepoMock.Verify(r => r.SoftDeleteAsync(5), Times.Once);
    }

    [Test]
    public async Task TC07_GetEffectiveRate_CalculatesBasePlusSurge_ForTargetDate()
    {
        // Arrange
        var targetDate = new DateOnly(2026, 12, 25); // Christmas surge
        var rate = new RoomRate
        {
            RateId = 1,
            RoomId = 101,
            BaseRate = 120m,
            SurgeRate = 50m,
            EffectiveFrom = new DateOnly(2026, 12, 20),
            EffectiveTo = new DateOnly(2026, 12, 31)
        };

        _rateRepoMock.Setup(r => r.GetEffectiveRateAsync(101, targetDate))
            .ReturnsAsync(rate);

        // Act
        var effectiveRate = await _rateService.GetEffectiveRateAsync(101, targetDate);

        // Assert
        // Nightly rate = BaseRate + SurgeRate = 120 + 50 = 170
        effectiveRate.Should().Be(170m);
    }

    [Test]
    public async Task TC07_GetEffectiveRate_ReturnsDefaultFallback_WhenNoRateConfiguredForDate()
    {
        // Arrange
        var targetDate = new DateOnly(2026, 5, 1);
        _rateRepoMock.Setup(r => r.GetEffectiveRateAsync(101, targetDate))
            .ReturnsAsync((RoomRate?)null);

        // Act
        var effectiveRate = await _rateService.GetEffectiveRateAsync(101, targetDate);

        // Assert: fallback rate is 3500.00m
        effectiveRate.Should().Be(3500.00m);
    }

    [Test]
    public async Task ServiceCatalog_SoftDelete_SetsIsAvailableFalse()
    {
        // Arrange
        _catalogRepoMock.Setup(c => c.GetByIdAsync(3))
            .ReturnsAsync(new RoomServiceItem { ServiceId = 3, ServiceName = "Laundry" });
        _catalogRepoMock.Setup(c => c.SoftDeleteAsync(3))
            .ReturnsAsync(true);

        // Act
        var result = await _catalogService.DeleteAsync(3);

        // Assert
        result.Should().BeTrue();
        _catalogRepoMock.Verify(c => c.SoftDeleteAsync(3), Times.Once);
    }
}
