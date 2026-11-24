using HotelMS.Reservations.DTOs;
using HotelMS.Reservations.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelMS.Reservations.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GuestsController : ControllerBase
{
    private readonly IGuestService _guestService;

    public GuestsController(IGuestService guestService)
    {
        _guestService = guestService;
    }

    [HttpGet]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> GetAll()
    {
        var guests = await _guestService.GetAllAsync();
        return Ok(guests);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> GetById(int id)
    {
        var guest = await _guestService.GetByIdAsync(id);
        return Ok(guest);
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> Create([FromBody] CreateGuestDto dto)
    {
        var created = await _guestService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.GuestId }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGuestDto dto)
    {
        var updated = await _guestService.UpdateAsync(id, dto);
        return Ok(updated);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpGet]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> GetAll()
    {
        var list = await _reservationService.GetAllAsync();
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _reservationService.GetByIdAsync(id);
        return Ok(item);
    }

    [HttpGet("code/{code}")]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var item = await _reservationService.GetByCodeAsync(code);
        return Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
    {
        var created = await _reservationService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.ReservationId }, created);
    }

    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> Cancel(int id)
    {
        await _reservationService.CancelAsync(id);
        return NoContent();
    }

    [HttpPost("{id:int}/services")]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> AddServiceItem(int id, [FromBody] AddServiceItemDto dto)
    {
        var item = await _reservationService.AddServiceItemAsync(id, dto);
        return Ok(item);
    }

    [HttpPost("availability")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckAvailability([FromBody] CheckAvailabilityRequestDto dto)
    {
        var available = await _reservationService.CheckAvailabilityAsync(dto);
        return Ok(available);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> GetAll()
    {
        var list = await _paymentService.GetAllAsync();
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _paymentService.GetByIdAsync(id);
        return Ok(item);
    }

    [HttpGet("reservation/{reservationId:int}")]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> GetByReservationId(int reservationId)
    {
        var list = await _paymentService.GetByReservationIdAsync(reservationId);
        return Ok(list);
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> ProcessPayment([FromBody] CreatePaymentDto dto)
    {
        var created = await _paymentService.ProcessPaymentAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.PaymentId }, created);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> GetAll()
    {
        var list = await _invoiceService.GetAllAsync();
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _invoiceService.GetByIdAsync(id);
        return Ok(item);
    }

    [HttpGet("reservation/{reservationId:int}")]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> GetByReservationId(int reservationId)
    {
        var item = await _invoiceService.GetByReservationIdAsync(reservationId);
        return Ok(item);
    }

    [HttpPost("generate/{reservationId:int}")]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> GenerateInvoice(int reservationId)
    {
        var invoice = await _invoiceService.GenerateInvoiceAsync(reservationId);
        return Ok(invoice);
    }
}
