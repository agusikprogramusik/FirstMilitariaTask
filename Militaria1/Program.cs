using Microsoft.Extensions.Configuration;
using Militaria1.Repositories;
using Militaria1.Services;

public class Program
{
    private static readonly HttpClient _client = new();
    private static IConfiguration _configuration;

    public static async Task Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        _configuration = builder.Build();

        var clientId = Environment.GetEnvironmentVariable("ALLEGRO_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("ALLEGRO_CLIENT_SECRET");
        string connectionString = _configuration.GetConnectionString("ConnectionS1");

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            Console.WriteLine("ALLEGRO_CLIENT_ID and ALLEGRO_CLIENT_SECRET environment variables must be set.");
            return;
        }

        ITokenService tokenService = new TokenService(_client, clientId, clientSecret);
        var token = await tokenService.GetTokenAsync();

        var billingEntryService = new BillingEntryService(_client);
        var billingEntries = await billingEntryService.GetBillingEntries(token);

        var billingEntryRepository = new BillingEntryRepository(connectionString);
        await billingEntryRepository.SaveBillingEntriesToDatabase(billingEntries);

        Console.WriteLine("Database updated properly.");
    }
}
