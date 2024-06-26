using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace Militaria1.Services;

public class TokenService : ITokenService
{
    private readonly HttpClient _client;
    private const string _tokenFilePath = "Resources/token.txt";
    private readonly string _clientId;
    private readonly string _clientSecret;

    public TokenService(HttpClient client, string clientId, string clientSecret)
    {
        _client = client;
        _clientId = clientId;
        _clientSecret = clientSecret;
    }

    public async Task<string> GetTokenAsync()
    {
        var token = ReadToken(_tokenFilePath);

        if (token == null || !(await IsTokenValidAsync(token)))
        {
            token = await Handle();
            SaveToken(token, _tokenFilePath);
        }

        return token;
    }

    private async Task<bool> IsTokenValidAsync(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("https://api.allegro.pl.allegrosandbox.pl/billing/billing-entries?limit=100");
        return response.IsSuccessStatusCode;
    }

    private async Task<string> Handle()
    {
        var deviceCodeResponse = await GetDeviceCode();
        var deviceCode = deviceCodeResponse.device_code.ToString();
        Console.WriteLine($"user code: {deviceCodeResponse.user_code}");
        Console.WriteLine($"verification uri: {deviceCodeResponse.verification_uri}");

        var accessToken = await WaitForAccessToken(deviceCode);
        return accessToken;
    }

    private async Task<dynamic> GetDeviceCode()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://allegro.pl.allegrosandbox.pl/auth/oauth/device");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_clientId}:{_clientSecret}")));

        var parameters = new[]
        {
            new KeyValuePair<string, string>("client_id", _clientId)
        };

        request.Content = new FormUrlEncodedContent(parameters);

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject(responseContent);
    }

    private async Task<string> WaitForAccessToken(string deviceCode)
    {
        while (true)
        {
            var tokenResponse = await GetAccessToken(deviceCode);
            if (tokenResponse != null && tokenResponse.access_token != null)
            {
                return tokenResponse.access_token;
            }
            await Task.Delay(5000);
        }
    }

    private async Task<dynamic> GetAccessToken(string deviceCode)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://allegro.pl.allegrosandbox.pl/auth/oauth/token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_clientId}:{_clientSecret}")));

        var parameters = new[]
        {
            new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:device_code"),
            new KeyValuePair<string, string>("device_code", deviceCode)
        };

        request.Content = new FormUrlEncodedContent(parameters);

        HttpResponseMessage response = await _client.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject(responseContent);
    }

    private void SaveToken(string token, string filePath)
    {
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
        System.IO.File.WriteAllText(filePath, token);
    }

    private string ReadToken(string filePath)
    {
        if (System.IO.File.Exists(filePath))
        {
            return System.IO.File.ReadAllText(filePath);
        }
        return null;
    }
}