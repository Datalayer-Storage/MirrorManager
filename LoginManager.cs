using Microsoft.AspNetCore.DataProtection;
using System.Dynamic;
using System.Text;
using System.Net.Http.Json;

public class LoginManager(IDataProtectionProvider provider,
                        AppStorage appStorage,
                        ILogger<LoginManager> logger,
                        IConfiguration configuration)
{
    private readonly IDataProtector _protector = provider.CreateProtector("DataLayer-Storage.datalayer.place.v1");
    private readonly AppStorage _appStorage = appStorage;
    private readonly ILogger<LoginManager> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    public async Task Login(CancellationToken stoppingToken = default)
    {
        try
        {
            var credentials = GetCredentials();
            if (!string.IsNullOrEmpty(credentials.accessToken) && !string.IsNullOrEmpty(credentials.secretKey))
            {
                Console.WriteLine("You are already logged in. Logout and try again.");
                return;
            }

            Console.WriteLine("In order to access the DataLayer API you must login.");
            Console.WriteLine("If you do not already have an access token and secret key visit https://datalayer.storage to create an account.\n");

            Console.Write("Access token: ");
            var accessToken = Console.ReadLine();
            if (string.IsNullOrEmpty(accessToken))
            {
                Console.WriteLine("Access token is required.");
                return;
            }

            Console.Write("Secret key: ");
            var secretKey = ReadSecret();
            if (string.IsNullOrEmpty(secretKey))
            {
                Console.WriteLine("Secret key is required.");
                return;
            }

            Console.WriteLine();

            // Encode the username and password with base64
            string encodedAuth = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(accessToken + ":" + secretKey));

            dynamic myPlace = await GetMyPlace(encodedAuth, stoppingToken);
            var token = myPlace?.proxy_key as object;
            if (string.IsNullOrEmpty(token?.ToString()))
            {
                Console.WriteLine("Login failed.");
                return;
            }

            // if we have gotten here we're good to go so securely store the token
            var protectedAuth = _protector.Protect(encodedAuth);
            _appStorage.Save("auth", protectedAuth);

            Console.WriteLine($"Proxy Key: {token}");
        }
        catch (Exception e)
        {
            _logger.LogError(e.InnerException?.Message ?? e.Message);
        }
    }

    public void LogOut()
    {
        _appStorage.Remove("auth");
    }

    public async Task ShowMyPlace(CancellationToken stoppingToken = default)
    {
        var (accessToken, secretKey) = GetCredentials();
        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(secretKey))
        {
            Console.WriteLine("Not logged in.");
            return;
        }

        string encodedAuth = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(accessToken + ":" + secretKey));

        dynamic myPlace = await GetMyPlace(encodedAuth, stoppingToken);

        var dictionary = (IDictionary<string, object>)myPlace;
        foreach (var pair in dictionary.Where(kvp => kvp.Key != "success").OrderBy(kvp => kvp.Key))
        {
            Console.WriteLine($"{pair.Key}: {pair.Value}");
        }
    }

    private async Task<dynamic> GetMyPlace(string encodedAuth, CancellationToken stoppingToken)
    {
        var loginUri = _configuration.GetValue("App:MeUri", "https://api.datalayer.storage/user/v1/me");
        _logger.LogInformation("Contacting {loginUri}", loginUri);

        // Set the Authorization header with the encoded username and password
        using var httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(60)
        };
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encodedAuth);

        var response = await httpClient.GetAsync(loginUri);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ExpandoObject>(stoppingToken) ?? throw new Exception("Login failed.");
    }

    private (string accessToken, string secretKey) GetCredentials()
    {
        var encodedAuth = _appStorage.Load("auth");
        if (string.IsNullOrEmpty(encodedAuth))
        {
            return (string.Empty, string.Empty);
        }

        var unprotected = _protector.Unprotect(encodedAuth);
        var decodedAuth = Encoding.GetEncoding("ISO-8859-1").GetString(Convert.FromBase64String(unprotected));
        var credentials = decodedAuth.Split(':');
        return (credentials[0], credentials[1]);
    }

    private static string ReadSecret()
    {
        var secret = "";
        while (true)
        {
            var key = Console.ReadKey(true);
            // Break the loop when Enter key is pressed
            if (key.Key == ConsoleKey.Enter)
            {
                break;
            }
            if (key.Key == ConsoleKey.Escape)
            {
                return string.Empty;
            }
            if (key.Key == ConsoleKey.Backspace)
            {
                if (secret.Length > 0)
                {
                    secret = secret.Remove(secret.Length - 1);
                    Console.Write("\b \b");
                }
                continue;
            }
            secret += key.KeyChar;
            Console.Write("*");
        }

        return secret;
    }
}
