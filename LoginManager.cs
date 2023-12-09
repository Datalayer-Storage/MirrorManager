using Microsoft.AspNetCore.DataProtection;
using System.Dynamic;
using System.Text;
using System.Net.Http.Json;

public class LoginManager(IDataProtectionProvider provider,
                        AppStorage appStorage,
                        DnsService dnsService,
                        ILogger<LoginManager> logger,
                        IConfiguration configuration)
{
    private readonly IDataProtector _protector = provider.CreateProtector("DataLayer-Storage.datalayer.place.v3");
    private readonly AppStorage _appStorage = appStorage;
    private readonly DnsService _dnsService = dnsService;
    private readonly ILogger<LoginManager> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    public async Task Login(CancellationToken stoppingToken = default)
    {
        try
        {
            var credentials = GetCredentials();
            if (!string.IsNullOrEmpty(credentials.accessToken) && !string.IsNullOrEmpty(credentials.secretKey))
            {
                _logger.LogWarning("You are already logged in. Logout and try again.");
                return;
            }

            Console.WriteLine("In order to access the DataLayer API you must login.");
            Console.WriteLine("If you do not already have an access token and secret key visit https://datalayer.storage to create an account.\n");

            Console.Write("Access token: ");
            var accessToken = Console.ReadLine();
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("Access token is required.");
                return;
            }

            Console.Write("Secret key: ");
            var secretKey = ReadSecret();
            if (string.IsNullOrEmpty(secretKey))
            {
                _logger.LogWarning("Secret key is required.");
                return;
            }

            Console.WriteLine();

            // Encode the username and password with base64
            string encodedAuth = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(accessToken + ":" + secretKey));

            dynamic myPlace = await GetMyPlace(encodedAuth, stoppingToken);
            var token = myPlace?.proxy_key as object;
            if (string.IsNullOrEmpty(token?.ToString()))
            {
                _logger.LogError("Login failed.");
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
        Console.WriteLine("You have been logged out.");
    }

    public async Task ShowMyPlace(CancellationToken stoppingToken = default)
    {
        var (accessToken, secretKey) = GetCredentials();
        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(secretKey))
        {
            _logger.LogWarning("Not logged in.");
            return;
        }

        var encodedAuth = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(accessToken + ":" + secretKey));

        dynamic myPlace = await GetMyPlace(encodedAuth, stoppingToken);

        var dictionary = (IDictionary<string, object>)myPlace;
        foreach (var pair in dictionary.Where(kvp => kvp.Key != "success").OrderBy(kvp => kvp.Key))
        {
            Console.WriteLine($"{pair.Key}: {pair.Value}");
        }
    }

    public async Task UpdateIP(CancellationToken stoppingToken = default)
    {
        var (accessToken, secretKey) = GetCredentials();
        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(secretKey))
        {
            _logger.LogWarning("Not logged in.");
            return;
        }

        var ip = await _dnsService.GetPublicIPAdress(stoppingToken);
        if (string.IsNullOrEmpty(ip))
        {
            _logger.LogError("Could not retrieve public IP address.");
            return;
        }

        try
        {
            _logger.LogInformation(ip);
            
            var updateIpUri = _configuration.GetValue("App:UserServiceUri", "https://api.datalayer.storage/user/v1/") + "update_user_ip";
            _logger.LogInformation("Contacting {loginUri}", updateIpUri);

            using var httpClient = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(60)
            };
            var encodedAuth = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(accessToken + ":" + secretKey));
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encodedAuth);

            var data = new { ip_address = ip };
            var response = await httpClient.PostAsJsonAsync(updateIpUri, data, stoppingToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(stoppingToken);

            Console.WriteLine(content);
        }
        catch (Exception e)
        {
            _logger.LogError(e.InnerException?.Message ?? e.Message);
        }
    }

    private async Task<dynamic> GetMyPlace(string encodedAuth, CancellationToken stoppingToken)
    {
        var loginUri = _configuration.GetValue("App:UserServiceUri", "https://api.datalayer.storage/user/v1/") + "me";
        _logger.LogInformation("Contacting {loginUri}", loginUri);

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
        var protectedAuth = _appStorage.Load("auth");
        if (string.IsNullOrEmpty(protectedAuth))
        {
            return (string.Empty, string.Empty);
        }

        var unprotected = _protector.Unprotect(protectedAuth);
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
            Console.Write("");
        }

        return secret;
    }
}
