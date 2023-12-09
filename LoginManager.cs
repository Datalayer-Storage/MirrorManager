using Microsoft.AspNetCore.DataProtection;

public class LoginManager(IDataProtectionProvider provider,
                        ILogger<HostManager> logger,
                        IConfiguration configuration)
{
    private readonly ILogger<HostManager> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IDataProtector _protector = provider.CreateProtector("DataLayer-Storage.DynDns.v1");
}
