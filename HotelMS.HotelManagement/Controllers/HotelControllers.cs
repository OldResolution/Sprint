using HotelMS.HotelManagement.DTOs;
using HotelMS.HotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelMS.HotelManagement.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = false) =>
        Ok(await _roomService.GetAllAsync(activeOnly));

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _roomService.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateRoomDto dto)
    {
        var result = await _roomService.CreateAsync(dto);
        return StatusCode(StatusCodes.Status201Created, new
        {
            message = "Room added successfully",
            roomId = result.RoomId,
            roomNumber = result.RoomNumber,
            roomType = result.RoomType,
            capacity = result.Capacity,
            status = result.Status,
            isActive = result.IsActive
        });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomDto dto) =>
        Ok(await _roomService.UpdateAsync(id, dto));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Delete(int id)
    {
        await _roomService.DeleteAsync(id);
        return NoContent();
    }
}

[ApiController]
[Route("api/rates")]
public class RatesController : ControllerBase
{
    private readonly IRateService _rateService;

    public RatesController(IRateService rateService)
    {
        _rateService = rateService;
    }

    [HttpGet]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> GetAll() =>
        Ok(await _rateService.GetAllAsync());

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _rateService.GetByIdAsync(id));

    [HttpGet("effective")]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> GetEffective([FromQuery] int roomId, [FromQuery] DateOnly date)
    {
        var rate = await _rateService.GetEffectiveRateAsync(roomId, date);
        return Ok(new { roomId, date, effectiveRate = rate });
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateRoomRateDto dto)
    {
        var result = await _rateService.CreateAsync(dto);
        return StatusCode(StatusCodes.Status201Created, new
        {
            message = "Room rate added successfully",
            rateId = result.RateId,
            roomId = result.RoomId,
            baseRate = result.BaseRate,
            surgeRate = result.SurgeRate
        });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomRateDto dto) =>
        Ok(await _rateService.UpdateAsync(id, dto));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Delete(int id)
    {
        await _rateService.DeleteAsync(id);
        return NoContent();
    }
}

[ApiController]
[Route("api/roomservices")]
public class RoomServicesController : ControllerBase
{
    private readonly IServiceCatalogService _service;

    public RoomServicesController(IServiceCatalogService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> GetAll([FromQuery] bool availableOnly = false) =>
        Ok(await _service.GetAllAsync(availableOnly));

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Owner,Manager,Receptionist")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateRoomServiceItemDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomServiceItemDto dto) =>
        Ok(await _service.UpdateAsync(id, dto));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}

[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _service;

    public InventoryController(IInventoryService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateInventoryItemDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInventoryItemDto dto) =>
        Ok(await _service.UpdateAsync(id, dto));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
