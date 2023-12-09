using System.Net.Http.Json;

public class HostManager(ILogger<HostManager> logger,
                        IConfiguration configuration)
{
    private readonly ILogger<HostManager> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    public async Task CheckHost(string host, CancellationToken token = default)
    {
        try
        {
            var hostToCheck = await GetHost(host, token);
            if (string.IsNullOrEmpty(hostToCheck))
            {
                _logger.LogWarning("No host specified and no public ip address found.");
            }
            else
            {
                using var httpClient = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };

                _logger.LogInformation($"Checking {hostToCheck}");

                var data = new { hostname = hostToCheck };
                var response = await httpClient.PostAsJsonAsync("https://api.datalayer.storage/mirrors/v1/check_connection", data, token);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync(token);

                _logger.LogInformation(content);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.InnerException?.Message ?? e.Message);
        }
    }

    private async Task<string?> GetHost(string host, CancellationToken token = default)
    {
        if (!string.IsNullOrEmpty(host))
        {
            return host;
        }

        string? variableValue = Environment.GetEnvironmentVariable("App:MirrorHostUri");
        if (!string.IsNullOrEmpty(variableValue))
        {
            return variableValue;
        }

        var publicIpAddress = await GetPublicIPAdress(token);
        if (!string.IsNullOrEmpty(publicIpAddress))
        {
            return $"http://{publicIpAddress}:8575";
        }

        return null;
    }

    private async Task<string> GetPublicIPAdress(CancellationToken stoppingToken)
    {
        try
        {
            using var httpClient = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            return await httpClient.GetStringAsync("https://api.ipify.org", stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get public ip address: {Message}", ex.InnerException?.Message ?? ex.Message);
            return string.Empty;
        }
    }
}
