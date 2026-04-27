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

            var addresses = results.Select(ParseNominatimResult).ToList();
            return Ok(addresses);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to contact Nominatim geocoding service");
            return StatusCode(503, "Geocoding service unavailable");
        }
    }

    /// <summary>
    /// Parses a Nominatim geocoding result into an Address model.
    /// </summary>
    private static Address ParseNominatimResult(NominatimResult result)
    {
        var latitude = ParseDouble(result.lat);
        var longitude = ParseDouble(result.lon);
        
        return new Address
        {
            DisplayName = result.display_name ?? string.Empty,
            Latitude = latitude,
            Longitude = longitude,
            Street = result.address?.road ?? string.Empty,
            City = result.address?.city ?? result.address?.town ?? result.address?.village ?? string.Empty,
            State = result.address?.state ?? string.Empty,
            Country = result.address?.country ?? string.Empty,
            PostalCode = result.address?.postcode ?? string.Empty
        };
    }

    /// <summary>
    /// Safely parses a string to double using invariant culture.
    /// </summary>
    private static double ParseDouble(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return 0;
        
        return double.TryParse(value, System.Globalization.NumberStyles.Float, 
            System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : 0;
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
