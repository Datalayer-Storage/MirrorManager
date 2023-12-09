
using chia.dotnet;
using System.CommandLine;
using System.CommandLine.Parsing;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<StoreManager>()
    .AddSingleton<HostManager>()
    .AddSingleton<LoginManager>()
    .AddSingleton<ChiaConfig>()
    .AddSingleton<RpcClientHost>()
    .AddSingleton((provider) => new AppStorage(".data-layer-storage"))
    .AddSingleton((provider) => new DataLayerProxy(provider.GetRequiredService<RpcClientHost>().GetRpcClient("data_layer"), "MirrorManager"))
    .AddDataProtection();

var host = builder.Build();
var rootCommand = Commands.CreateCommands(host.Services);
return await rootCommand.InvokeAsync(args);
