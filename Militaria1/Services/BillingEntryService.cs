using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace Militaria1.Services;

public class BillingEntryService
{
    private readonly HttpClient _client;

    public BillingEntryService(HttpClient client)
    {
        _client = client;
    }

    public async Task<List<JObject>> GetBillingEntries(string accessToken)
    {
        List<JObject> allEntries = new List<JObject>();
        var url = "https://api.allegro.pl.allegrosandbox.pl/billing/billing-entries?limit=10";

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        _client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.allegro.public.v1+json"));

        while (url != null)
        {
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseJson = JObject.Parse(responseBody);

            var entries = (JArray)responseJson["billingEntries"];
            allEntries.AddRange(entries.Cast<JObject?>());

            var nextLink = responseJson["links"]?["next"]?["href"];
            url = nextLink?.ToString();
        }

        return allEntries;
    }
}