using LoadoutComunication;
using Newtonsoft.Json;

namespace Matchmaking.Source.Services
{
    // Typed HttpClient — registered via AddHttpClient<DbWrapperClient> in Program.cs.
    // The base address and X-Internal-Key header are configured at registration time.
    public class DbWrapperClient
    {
        readonly HttpClient _client;

        public DbWrapperClient(HttpClient client) => _client = client;

        public async Task<(UserStatsDTO? stats, string? error)> GetStats(string userId)
        {
            try
            {
                var resp = await _client.GetAsync($"/stats/{userId}");
                var body = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode)
                    return (null, $"Stats fetch failed ({(int)resp.StatusCode}): {body}");
                return (JsonConvert.DeserializeObject<UserStatsDTO>(body), null);
            }
            catch (Exception ex) { return (null, ex.Message); }
        }

        public async Task<(LoadoutDTO? loadout, string? error)> GetLoadout(string userId)
        {
            try
            {
                var resp = await _client.GetAsync($"/loadout/{userId}");
                var body = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode)
                    return (null, $"Loadout fetch failed ({(int)resp.StatusCode}): {body}");
                return (JsonConvert.DeserializeObject<LoadoutDTO>(body), null);
            }
            catch (Exception ex) { return (null, ex.Message); }
        }
    }
}
