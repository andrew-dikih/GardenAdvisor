using Microsoft.AspNetCore.Mvc;
using GardenAdvisor.API.Models;
using System.Text.Json;

namespace GardenAdvisor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AddressController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AddressController> _logger;

    public AddressController(IHttpClientFactory httpClientFactory, ILogger<AddressController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchAddress([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query is required");

        try
        {
            var client = _httpClientFactory.CreateClient("nominatim");
            var encodedQuery = Uri.EscapeDataString(query);
            var response = await client.GetAsync($"search?q={encodedQuery}&format=json&addressdetails=1&limit=5");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Nominatim geocoding service returned {StatusCode}", response.StatusCode);
                return StatusCode((int)response.StatusCode, "Geocoding service error");
            }

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var results = JsonSerializer.Deserialize<List<NominatimResult>>(content, options);

            if (results == null || results.Count == 0)
                return Ok(new List<Address>());

            var addresses = results.Select(r => new Address
            {
                DisplayName = r.display_name ?? string.Empty,
                Latitude = double.TryParse(r.lat, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat) ? lat : 0,
                Longitude = double.TryParse(r.lon, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lon) ? lon : 0,
                Street = r.address?.road ?? string.Empty,
                City = r.address?.city ?? r.address?.town ?? r.address?.village ?? string.Empty,
                State = r.address?.state ?? string.Empty,
                Country = r.address?.country ?? string.Empty,
                PostalCode = r.address?.postcode ?? string.Empty
            }).ToList();

            return Ok(addresses);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to contact Nominatim geocoding service");
            return StatusCode(503, "Geocoding service unavailable");
        }
    }
}

public class NominatimResult
{
    public string? display_name { get; set; }
    public string? lat { get; set; }
    public string? lon { get; set; }
    public NominatimAddress? address { get; set; }
}

public class NominatimAddress
{
    public string? road { get; set; }
    public string? city { get; set; }
    public string? town { get; set; }
    public string? village { get; set; }
    public string? state { get; set; }
    public string? country { get; set; }
    public string? postcode { get; set; }
}
