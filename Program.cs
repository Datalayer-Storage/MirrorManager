using System.CommandLine;
using System.CommandLine.Parsing;

namespace MirrorManager;

static class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Manages local chia data layer mirrors and subscriptions.");

        var unsubscribeCommand = CreateCommandWithSingleOption(
            "unsubscribe",
            "Unsubscribe from all stores.",
            "retain",
            "Whether to retain files when unsubscribing.",
            false,
            retain => StoreManager.UnsubscribeAll(retain));

        var unmirrorCommand = CreateCommandWithSingleOption(
            "unmirror",
            "Unmirror all stores.",
            "fee",
            "Fee to use for each removal transaction.",
            0UL,
            fee => StoreManager.UnmirrorAll(fee));

        var listAllCommand = CreateCommandWithSingleOption(
            "list-all",
            "List all stores and their mirrors.",
            "ours",
            "Whether to only list our mirrors.",
            true,
            ours => StoreManager.ListAll(ours));

        var checkCommand = CreateCommandWithSingleOption(
            "check",
            "Verify that a mirror host is accessible.",
            "host",
            "The host address to check. Omit to check the local mirror.",
            string.Empty,
            host => HostManager.CheckHost(host));

        rootCommand.AddCommand(checkCommand);
        rootCommand.AddCommand(listAllCommand);
        rootCommand.AddCommand(unmirrorCommand);
        rootCommand.AddCommand(unsubscribeCommand);

        return await rootCommand.InvokeAsync(args);
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
