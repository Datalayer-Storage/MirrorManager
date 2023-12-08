namespace MirrorManager;

using Microsoft.AspNetCore.DataProtection;

public class LoginManager
{
    IDataProtector _protector;

    public LoginManager(IDataProtectionProvider provider) => _protector = provider.CreateProtector("Contoso.MyClass.v1");
}
