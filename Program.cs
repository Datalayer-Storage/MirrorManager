using System.CommandLine;
using System.CommandLine.Parsing;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace MirrorManager;

static class Program
{
    static async Task<int> Main(string[] args)
    {
        var servicesCollection = new ServiceCollection();
        servicesCollection.AddDataProtection();
        servicesCollection.AddSingleton<StoreManager>();

        var rootCommand = CreateCommands(servicesCollection.BuildServiceProvider());
        return await rootCommand.InvokeAsync(args);
    }

    private static RootCommand CreateCommands(ServiceProvider services)
    {

        var rootCommand = new RootCommand("Manages local chia data layer mirrors and subscriptions.");

        var commands = new List<Command>()
        {
            CreateCommandWithSingleOption(
                "unsubscribe",
                "Unsubscribe from all stores.",
                "retain",
                "Whether to retain files when unsubscribing.",
                false,
                async retain => {
                    var stores = services.GetRequiredService<StoreManager>();
                    await stores.UnsubscribeAll(retain);
                }),
            CreateCommandWithSingleOption(
                "unmirror",
                "Unmirror all stores.",
                "fee",
                "Fee to use for each removal transaction.",
                0UL,
                async fee => {
                    var stores = services.GetRequiredService<StoreManager>();
                    await stores.UnmirrorAll(fee);
                }),
            CreateCommandWithSingleOption(
                "list-all",
                "List all stores and their mirrors.",
                "ours",
                "Whether to only list our mirrors.",
                true,
                ours => StoreManager.ListAll(ours))         ,
            CreateCommandWithSingleOption(
                "check",
                "Verify that a mirror host is accessible.",
                "host",
                "The host address to check. Omit to check the local mirror.",
                string.Empty,
                host => HostManager.CheckHost(host))
        };

        commands.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

        foreach (var command in commands)
        {
            rootCommand.AddCommand(command);
        }

        return rootCommand;
    }

    private static Command CreateCommandWithSingleOption<TOption>(string commandName, string commandDescription, string optionName, string optionDescription, TOption optionDefault, Func<TOption, Task> handler)
    {
        var command = new Command(commandName, commandDescription);
        var option = new Option<TOption>(
            new[] { $"--{optionName}", $"-{optionName[0]}" },
            getDefaultValue: () => optionDefault,
            description: optionDescription);
        command.AddOption(option);
        command.SetHandler(async (optionValue) =>
        {
            await handler(optionValue);
        }, option);

        return command;
    }
}
