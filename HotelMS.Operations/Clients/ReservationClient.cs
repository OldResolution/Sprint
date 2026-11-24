using System.Net.Http.Json;

namespace HotelMS.Operations.Clients;

public record RemotePaymentDto(
    int PaymentId,
    int ReservationId,
    decimal Amount,
    string PaymentMethod,
    string PaymentStatus,
    string? TransactionReference,
    DateTime PaymentDate
);

public record RemoteInvoiceDto(
    int InvoiceId,
    int ReservationId,
    string InvoiceNumber,
    DateTime IssueDate,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    string InvoiceStatus
);

public interface IReservationClient
{
    Task<IEnumerable<RemotePaymentDto>> GetPaymentsAsync();
    Task<IEnumerable<RemoteInvoiceDto>> GetInvoicesAsync();
}

public class ReservationClient : IReservationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ReservationClient> _logger;

    public ReservationClient(HttpClient httpClient, ILogger<ReservationClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<RemotePaymentDto>> GetPaymentsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<RemotePaymentDto>>("api/payments");
            return response ?? Enumerable.Empty<RemotePaymentDto>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve payments from ReservationService");
            return Enumerable.Empty<RemotePaymentDto>();
        }
    }

    public async Task<IEnumerable<RemoteInvoiceDto>> GetInvoicesAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<RemoteInvoiceDto>>("api/invoices");
            return response ?? Enumerable.Empty<RemoteInvoiceDto>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve invoices from ReservationService");
            return Enumerable.Empty<RemoteInvoiceDto>();
        }
    }
}
