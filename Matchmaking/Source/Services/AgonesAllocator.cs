using k8s;
using MatchmakingComunication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedUtils.Source.Logging;

namespace Matchmaking.Source.Services
{
    public record AllocatedServer(string Ip, int Port);

    public class AgonesAllocator
    {
        const string FleetName = "battle-server-fleet";
        const string Namespace = "primal-conquest";

        readonly IKubernetes                    _k8s;

        public AgonesAllocator()
        {
            var config = KubernetesClientConfiguration.IsInCluster()
                ? KubernetesClientConfiguration.InClusterConfig()
                : KubernetesClientConfiguration.BuildConfigFromConfigFile();
            _k8s = new Kubernetes(config);
        }

        // Creates a GameServerAllocation CRD and reads back the assigned IP and port.
        // Loadout data is passed as annotations so the BattleServer can read them
        // from its own GameServer metadata on startup.
        public async Task<(MatchFoundDTO? server, string? error)> AllocateAsync(
            Dictionary<string, string> annotations)
        {
            var body = new
            {
                apiVersion = "allocation.agones.dev/v1",
                kind       = "GameServerAllocation",
                spec = new
                {
                    selectors = new[]
                    {
                        new { matchLabels = new Dictionary<string, string>
                            { ["agones.dev/fleet"] = FleetName } }
                    },
                    metadata = new { annotations }
                }
            };

            try
            {
                var result = await _k8s.CustomObjects.CreateNamespacedCustomObjectAsync(
                    body:               body,
                    group:              "allocation.agones.dev",
                    version:            "v1",
                    namespaceParameter: Namespace,
                    plural:             "gameserverallocations");

                var jObj  = JObject.Parse(JsonConvert.SerializeObject(result));
                var state = jObj["status"]?["state"]?.ToString();

                if (state != "Allocated")
                    return (null, $"Allocation state was '{state ?? "unknown"}' — no free servers");

                var ip   = jObj["status"]!["address"]!.ToString();
                var port = jObj["status"]!["ports"]![0]!["port"]!.Value<int>();

                return (new MatchFoundDTO {Ip= ip, Port=port }, null);
            }
            catch (Exception ex)
            {
                LogSystem.Log(ELogCategory.Debug, ELogLevel.Error, "Agones GameServerAllocation failed");
                return (null, ex.Message);
            }
        }
    }
}
