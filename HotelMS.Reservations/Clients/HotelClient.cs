using System.Net.Http.Json;

namespace HotelMS.Reservations.Clients;

public record RoomInfoResponse(
    int RoomId,
    string RoomNumber,
    string RoomType,
    int Capacity,
    string Status,
    bool IsActive
);

public record RoomServiceItemResponse(
    int ServiceId,
    string ServiceName,
    string? Description,
    decimal Price,
    bool IsAvailable
);

public interface IHotelClient
{
    Task<RoomInfoResponse?> GetRoomAsync(int roomId);
    Task<decimal> GetEffectiveRateAsync(int roomId, DateOnly date);
    Task<RoomServiceItemResponse?> GetServiceItemAsync(int serviceId);
    Task<IEnumerable<RoomInfoResponse>> GetAllRoomsAsync(bool activeOnly = true);
}

public class HotelClient : IHotelClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HotelClient> _logger;

    public HotelClient(HttpClient httpClient, ILogger<HotelClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<RoomInfoResponse?> GetRoomAsync(int roomId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<RoomInfoResponse>($"api/rooms/{roomId}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch room {RoomId} from HotelManagementService", roomId);
            return null;
        }
    }

    public async Task<decimal> GetEffectiveRateAsync(int roomId, DateOnly date)
    {
        try
        {
            var formattedDate = date.ToString("yyyy-MM-dd");
            var response = await _httpClient.GetFromJsonAsync<decimal>($"api/rates/effective?roomId={roomId}&date={formattedDate}");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch effective rate for room {RoomId} on {Date}. Defaulting to 100", roomId, date);
            return 100m; // Fallback safe default
        }
    }

    public async Task<RoomServiceItemResponse?> GetServiceItemAsync(int serviceId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<RoomServiceItemResponse>($"api/roomservices/{serviceId}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch service item {ServiceId} from HotelManagementService", serviceId);
            return null;
        }
    }

    public async Task<IEnumerable<RoomInfoResponse>> GetAllRoomsAsync(bool activeOnly = true)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<RoomInfoResponse>>($"api/rooms?activeOnly={activeOnly}");
            return response ?? Enumerable.Empty<RoomInfoResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch rooms from HotelManagementService");
            return Enumerable.Empty<RoomInfoResponse>();
        }
    }
}
