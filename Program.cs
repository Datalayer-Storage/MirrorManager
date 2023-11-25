using System.CommandLine;
using System.CommandLine.Parsing;

namespace MirrorManager;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Manages local chia data layer mirrors and subscriptions.");

        var unsubscribeCommand = new Command("unsubscribe-all", "Unsubscribes from all stores.");
        var retainOption = new Option<bool>(
            new[] { "--retain", "-r" },
            getDefaultValue: () => false,
            description: "Whether to retain files when unsubscribing.");
        unsubscribeCommand.AddOption(retainOption);
        unsubscribeCommand.SetHandler(async (retain) =>
        {
            await StoreManager.UnsubscribeAll(retain);
        }, retainOption);

        var unMirrorCommand = new Command("unmirror-all", "Unmirrors all stores.");
        var feeOption = new Option<ulong>(
            new[] { "--fee", "-f" },
            getDefaultValue: () => 0,
            description: "Fee to use for each removal transaction.");
        unMirrorCommand.AddOption(feeOption);
        unMirrorCommand.SetHandler(async (fee) =>
        {
            await StoreManager.UnmirrorAll(fee);
        }, feeOption);

        var listAllCommand = new Command("list-all", "List all stores and their mirrors.");
        var oursOnlyOption = new Option<bool>(
            new[] { "--ours", "-o" },
            getDefaultValue: () => true,
            description: "Whether to only list our mirrors.");
        listAllCommand.AddOption(oursOnlyOption);
        listAllCommand.SetHandler(async (ours) =>
        {
            await StoreManager.ListAll(ours);
        }, oursOnlyOption);

        var checkCommand = new Command("check", "Verify that a mirror is accessible.");
        var hostOption = new Option<string>(
            new[] { "--host", "-h" },
            getDefaultValue: () => "",
            description: "The host address to check. Omit to check the local mirror.");
        checkCommand.AddOption(hostOption);
        checkCommand.SetHandler(async (host) =>
        {
            await HostManager.CheckHost(host);
        }, hostOption);

        rootCommand.AddCommand(checkCommand);
        rootCommand.AddCommand(listAllCommand);
        rootCommand.AddCommand(unMirrorCommand);
        rootCommand.AddCommand(unsubscribeCommand);

        return await rootCommand.InvokeAsync(args);
    }
}
