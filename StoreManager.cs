namespace MirrorManager;

using chia.dotnet;

public class StoreManager
{
    public StoreManager()
    {
    }
    
    public async Task UnsubscribeAll(bool retain, CancellationToken token = default)
    {
        var dataLayer = GetDataLayer();
        var subscriptions = await dataLayer.Subscriptions(token);
        foreach (var subscription in subscriptions)
        {
            Console.WriteLine($"Removing subscription {subscription}");
            await dataLayer.Unsubscribe(subscription, retain, token);
        }
    }

    public async Task UnmirrorAll(ulong fee, CancellationToken token = default)
    {
        var dataLayer = GetDataLayer();
        try
        {
            var subscriptions = await dataLayer.Subscriptions(token);
            foreach (var subscription in subscriptions)
            {
                var mirrors = await dataLayer.GetMirrors(subscription, token);

                foreach (var mirror in mirrors.Where(m => m.Ours))
                {
                    Console.WriteLine($"Removing mirror for {subscription}");
                    await dataLayer.DeleteMirror(mirror.CoinId, fee, token);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.InnerException?.Message ?? e.Message);
        }
    }

    public static async Task ListAll(bool ours, CancellationToken token = default)
    {
        var dataLayer = GetDataLayer();
        try
        {
            var subscriptions = await dataLayer.Subscriptions(token);
            Console.WriteLine($"Found {subscriptions.Count()} subscriptions.\n");

            foreach (var subscription in subscriptions)
            {
                Console.WriteLine($"Subscription: {subscription}");
                var mirrors = await dataLayer.GetMirrors(subscription, token);
                mirrors = ours ? mirrors.Where(m => m.Ours) : mirrors;

                foreach (var mirror in mirrors)
                {
                    Console.WriteLine($"  Mirror: {mirror}");
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.InnerException?.Message ?? e.Message);
        }
    }


    private static DataLayerProxy GetDataLayer()
    {
        var config = Config.Open();
        var endpoint = config.GetEndpoint("data_layer");
        Console.WriteLine($"Using data layer at {endpoint.Uri}");
        var rpcClient = new HttpRpcClient(endpoint);

        return new DataLayerProxy(rpcClient, "MirrorManager");
    }
}
