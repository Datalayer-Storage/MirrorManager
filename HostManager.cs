namespace MirrorManager;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

public static class HostManager
{
    public static async Task CheckHost(string host, CancellationToken token = default)
    {
        try
        {
            var hostToCheck = await GetHost(host, token);
            if (string.IsNullOrEmpty(hostToCheck))
            {
                Console.WriteLine("No host specified and no public ip address found.");
            }
            else
            {
                using var httpClient = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };

                var data = new { hostname = hostToCheck };
                var response = await httpClient.PostAsJsonAsync("https://api.datalayer.storage/mirrors/v1/check_connection", data, token);
                response.EnsureSuccessStatusCode();
                var content = await JsonSerializer.DeserializeAsync<dynamic>(await response.Content.ReadAsStreamAsync(token), cancellationToken: token);
                Console.WriteLine(content);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.InnerException?.Message ?? e.Message);
        }
    }

    private static async Task<string?> GetHost(string host, CancellationToken token = default)
    {
        if (!string.IsNullOrEmpty(host))
        {
            return host;
        }

        string? variableValue = Environment.GetEnvironmentVariable("DlMirrorSync:MirrorHostUri");
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

    private static async Task<string> GetPublicIPAdress(CancellationToken stoppingToken)
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
            Console.WriteLine("Failed to get public ip address: {Message}", ex.Message);
            return string.Empty;
        }
    }
}
