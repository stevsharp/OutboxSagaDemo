
using ConsoleAppTester;

using System.Net.Http.Json;
using System.Text.Json;


// Run: dotnet run
var baseUrl = "https://localhost:7128";

using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };

var jsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

// 1) Checkout → get OrderId (expects JSON with orderId/OrderId)
var checkoutBody = new CheckoutRequest("test@example.com", 49.99m);
var checkoutResp = await http.PostAsJsonAsync("/checkout", checkoutBody);
await EnsureSuccess(checkoutResp, "checkout");

var checkoutJson = await checkoutResp.Content.ReadAsStringAsync();
Console.WriteLine($"Checkout raw: {checkoutJson}");

var checkoutData = TryDeserialize<CheckoutResponse>(checkoutJson, jsonOpts)
    ?? throw new InvalidOperationException("Checkout returned no JSON or unexpected shape.");
var orderId = checkoutData.OrderId;
Console.WriteLine($"OrderId: {orderId}");

// 2) Stock reserved (202 with empty body is fine)
var stockResp = await http.PostAsync($"/stock/{orderId}/reserved", content: null);
await EnsureSuccess(stockResp, "stock reserved");
Console.WriteLine($"Stock reserved ✓ ({(int)stockResp.StatusCode})");

// 3) Payment authorized (may return JSON or be empty)
var payResp = await http.PostAsync($"/payment/{orderId}/authorized", content: null);
await EnsureSuccess(payResp, "payment authorized");

var payRaw = await payResp.Content.ReadAsStringAsync();
Console.WriteLine($"Payment raw: {payRaw}");

var payData = TryDeserialize<PaymentResponse>(payRaw, jsonOpts);
if (payData is not null)
    Console.WriteLine($"Payment authorized ✓ Code: {payData.AuthorizationCode}");
else
    Console.WriteLine("Payment authorized ✓ (no JSON body)");

// done
Console.WriteLine("Done.");

Console.ReadLine();

static async Task EnsureSuccess(HttpResponseMessage resp, string step)
{
    if (!resp.IsSuccessStatusCode)
    {
        var body = await resp.Content.ReadAsStringAsync();
        throw new HttpRequestException($"{step} failed: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{body}");
    }
}

static T? TryDeserialize<T>(string? json, JsonSerializerOptions opts) where T : class
{
    if (string.IsNullOrWhiteSpace(json))
        return null;

    try { return JsonSerializer.Deserialize<T>(json, opts); }
    catch { return null; }
}
