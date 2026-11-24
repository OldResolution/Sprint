using FluentAssertions;
using HotelMS.Operations.Clients;
using HotelMS.Operations.DTOs;
using HotelMS.Operations.Exceptions;
using HotelMS.Operations.Models;
using HotelMS.Operations.Repositories.Interfaces;
using HotelMS.Operations.Services.Classes;
using Moq;
using NUnit.Framework;

namespace HotelMS.Operations.Tests;

[TestFixture]
public class OperationsServiceTests
{
    private Mock<IDepartmentRepository> _deptRepoMock = null!;
    private Mock<IStaffRepository> _staffRepoMock = null!;
    private Mock<IReservationClient> _resClientMock = null!;

    private DepartmentService _deptService = null!;
    private StaffService _staffService = null!;
    private ReportService _reportService = null!;

    [SetUp]
    public void SetUp()
    {
        _deptRepoMock = new Mock<IDepartmentRepository>();
        _staffRepoMock = new Mock<IStaffRepository>();
        _resClientMock = new Mock<IReservationClient>();

        _deptService = new DepartmentService(_deptRepoMock.Object);
        _staffService = new StaffService(_staffRepoMock.Object, _deptRepoMock.Object);
        _reportService = new ReportService(_staffRepoMock.Object, _deptRepoMock.Object, _resClientMock.Object);
    }

    [Test]
    public async Task DeleteDepartment_WithAssignedStaff_ThrowsConflictException()
    {
        // Arrange
        var dept = new Department { DepartmentId = 1, DepartmentName = "Front Desk" };
        _deptRepoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync(dept);
        _deptRepoMock.Setup(d => d.HasStaffAsync(1)).ReturnsAsync(true); // Staff assigned!

        // Act
        var act = async () => await _deptService.DeleteAsync(1);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot delete department because staff members are assigned to it.");
    }

    [Test]
    public async Task DeleteDepartment_WithoutStaff_Succeeds()
    {
        // Arrange
        var dept = new Department { DepartmentId = 2, DepartmentName = "Archive" };
        _deptRepoMock.Setup(d => d.GetByIdAsync(2)).ReturnsAsync(dept);
        _deptRepoMock.Setup(d => d.HasStaffAsync(2)).ReturnsAsync(false);
        _deptRepoMock.Setup(d => d.DeleteAsync(2)).ReturnsAsync(true);

        // Act
        var result = await _deptService.DeleteAsync(2);

        // Assert
        result.Should().BeTrue();
        _deptRepoMock.Verify(d => d.DeleteAsync(2), Times.Once);
    }

    [Test]
    public async Task CreateStaff_WithDuplicateNIC_ThrowsConflictException()
    {
        // Arrange
        var dept = new Department { DepartmentId = 1, IsActive = true };
        _deptRepoMock.Setup(d => d.GetByIdAsync(1)).ReturnsAsync(dept);
        _staffRepoMock.Setup(s => s.GetByNicAsync("NIC-1111", null))
            .ReturnsAsync(new Staff { StaffId = 9, NIC = "NIC-1111" });

        var dto = new CreateStaffDto(1, "John Doe", "123 St", "NIC-1111", 40000m, 30, "Porter", "john@hotel.com");

        // Act
        var act = async () => await _staffService.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Staff member with NIC 'NIC-1111' already exists.");
    }

    [Test]
    public async Task CreateStaff_WithInactiveDepartment_ThrowsBadRequestException()
    {
        // Arrange
        var dept = new Department { DepartmentId = 3, IsActive = false }; // Inactive department
        _deptRepoMock.Setup(d => d.GetByIdAsync(3)).ReturnsAsync(dept);

        var dto = new CreateStaffDto(3, "John Doe", "123 St", "NIC-2222", 40000m, 30, "Porter", "john@hotel.com");

        // Act
        var act = async () => await _staffService.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Cannot assign staff to an inactive department.");
    }

    [Test]
    public async Task GetStaffSalaryReport_AggregatesSalariesCorrectly()
    {
        // Arrange
        var dept1 = new Department { DepartmentId = 1, DepartmentName = "IT" };
        var dept2 = new Department { DepartmentId = 2, DepartmentName = "Sales" };
        _deptRepoMock.Setup(d => d.GetAllAsync(false)).ReturnsAsync(new List<Department> { dept1, dept2 });

        var staffList = new List<Staff>
        {
            new Staff { StaffId = 1, DepartmentId = 1, Salary = 50000m },
            new Staff { StaffId = 2, DepartmentId = 1, Salary = 60000m },
            new Staff { StaffId = 3, DepartmentId = 2, Salary = 40000m }
        };
        _staffRepoMock.Setup(s => s.GetAllAsync()).ReturnsAsync(staffList);

        // Act
        var report = await _reportService.GetStaffSalaryReportAsync();

        // Assert
        report.Should().NotBeNull();
        report.TotalStaffCount.Should().Be(3);
        report.TotalSalaryExpense.Should().Be(150000m);
        report.Departments.Should().HaveCount(2);

        var itDept = report.Departments.First(d => d.DepartmentId == 1);
        itDept.StaffCount.Should().Be(2);
        itDept.TotalSalary.Should().Be(110000m);
    }

    [Test]
    public async Task GetHotelIncomeReport_AggregatesPaymentsAndInvoices()
    {
        // Arrange
        var payments = new List<RemotePaymentDto>
        {
            new RemotePaymentDto(1, 10, 200m, "Card", "Successful", "TX1", DateTime.UtcNow),
            new RemotePaymentDto(2, 11, 300m, "UPI", "Successful", "TX2", DateTime.UtcNow),
            new RemotePaymentDto(3, 12, 100m, "Card", "Failed", "TX3", DateTime.UtcNow) // Failed payment not included in revenue
        };
        _resClientMock.Setup(c => c.GetPaymentsAsync()).ReturnsAsync(payments);

        var invoices = new List<RemoteInvoiceDto>
        {
            new RemoteInvoiceDto(1, 10, "INV-1", DateTime.UtcNow, 200m, 24m, 224m, "Paid"),
            new RemoteInvoiceDto(2, 11, "INV-2", DateTime.UtcNow, 300m, 36m, 336m, "Pending")
        };
        _resClientMock.Setup(c => c.GetInvoicesAsync()).ReturnsAsync(invoices);

        // Act
        var report = await _reportService.GetHotelIncomeReportAsync();

        // Assert
        report.Should().NotBeNull();
        report.TotalRevenue.Should().Be(500m);
        report.SuccessfulPaymentsCount.Should().Be(2);
        report.RevenueByPaymentMethod["Card"].Should().Be(200m);
        report.RevenueByPaymentMethod["UPI"].Should().Be(300m);
        report.PaidInvoicesCount.Should().Be(1);
        report.PendingInvoicesCount.Should().Be(1);
    }
}
