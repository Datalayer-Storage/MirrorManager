using System.CommandLine;

static class Commands
{
    public static RootCommand CreateCommands(IServiceProvider services)
    {
        var rootCommand = new RootCommand("Manages local chia data layer mirrors and subscriptions.");

        var checkCommand = new Command("check", "Verify that a mirror host is accessible.");
        var hostArgument = new Argument<string>("host", () => string.Empty, "The host address to check. Omit to check the local mirror.");
        checkCommand.AddArgument(hostArgument);
        checkCommand.SetHandler(async (host) =>
        {
            var hostManager = services.GetRequiredService<HostManager>();
            await hostManager.CheckHost(host);
        }, hostArgument);

        var loginCommand = new Command("login", "Log in to datalayer.place.");
        loginCommand.SetHandler(async () =>
        {
            var loginManager = services.GetRequiredService<LoginManager>();
            await loginManager.Login();
        });

        var showMyPlaceCommand = new Command("show", "Show datalayer.place details.");
        showMyPlaceCommand.SetHandler(async () =>
        {
            var loginManager = services.GetRequiredService<LoginManager>();
            await loginManager.ShowMyPlace();
        });

        var logoutCommand = new Command("logout", "Log out of datalayer.place.");
        logoutCommand.SetHandler(() =>
        {
            var loginManager = services.GetRequiredService<LoginManager>();
            loginManager.LogOut();
        });

        var updateIpCommand = new Command("update", "Update the ip address for your datalayer.place proxy.");
        updateIpCommand.SetHandler(async () =>
        {
            var loginManager = services.GetRequiredService<LoginManager>();
            await loginManager.UpdateIP();
        });

        var commands = new List<Command>()
        {
            checkCommand,
            loginCommand,
            showMyPlaceCommand,
            logoutCommand,
            updateIpCommand,
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
                "List all subscribed stores and their mirrors.",
                "ours",
                "Whether to only list our mirrors.",
                true,
                async ours => {
                    var stores = services.GetRequiredService<StoreManager>();
                    await stores.ListAll(ours);
                }),
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
