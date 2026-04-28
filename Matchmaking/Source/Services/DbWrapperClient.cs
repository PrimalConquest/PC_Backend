using LoadoutComunication;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Matchmaking.Source.Services
{
    // Typed HttpClient — registered via AddHttpClient<DbWrapperClient> in Program.cs.
    // The base address and X-Internal-Key header are configured at registration time.
    public class DbWrapperClient
    {
        readonly HttpClient _client;
        readonly ILogger<DbWrapperClient> _log;

        public DbWrapperClient(HttpClient client, ILogger<DbWrapperClient> log)
        {
            _client = client;
            _log    = log;
        }

        public async Task<(UserStatsDTO? stats, string? error)> GetStats(string userId)
        {
            var url = $"/stats/{userId}";
            _log.LogInformation("GetStats → {BaseAddress}{Url}", _client.BaseAddress, url);
            try
            {
                var resp = await _client.GetAsync(url);
                var body = await resp.Content.ReadAsStringAsync();
                _log.LogInformation("GetStats ← {Status}: {Body}", (int)resp.StatusCode, body);
                if (!resp.IsSuccessStatusCode)
                    return (null, $"Stats fetch failed ({(int)resp.StatusCode}): {body}");
                return (JsonConvert.DeserializeObject<UserStatsDTO>(body), null);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "GetStats threw for userId={UserId}", userId);
                return (null, ex.Message);
            }
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
