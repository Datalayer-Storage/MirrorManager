[Command("stores", Description = "Manage subscriptions and mirrors.")]
internal sealed class StoreCommands
{
    [Command("unsubscribe-all", Description = "Unsubscribe from all stores.")]
    public UnsubscribeAllCommand CheckHost { get; init; } = new();

    [Command("unmirror-all", Description = "Unmirror all subscribed stores.")]
    public UnmirrorAllCommand Unmirror { get; init; } = new();

    [Command("list", Description = "List subscribed stores and their mirrors.")]
    public ListAllCommand List { get; init; } = new();
}
