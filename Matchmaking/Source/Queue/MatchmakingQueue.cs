using System.Collections.Concurrent;

namespace Matchmaking.Source.Queue
{
    // Thread-safe in-memory queue. Keyed by UserId so re-queuing is idempotent.
    public class MatchmakingQueue
    {
        readonly ConcurrentDictionary<string, QueueEntry> _entries = new();

        public void Enqueue(QueueEntry entry) => _entries[entry.UserId] = entry;

        public void Dequeue(string userId) => _entries.TryRemove(userId, out _);

        public bool TryDequeue(string userId, out QueueEntry entry) =>
            _entries.TryRemove(userId, out entry!);

        // Sorted ascending by rank so the background service can sweep linearly.
        public IReadOnlyList<QueueEntry> GetAllSortedByRank() =>
            _entries.Values.OrderBy(e => e.RankPoints).ToList();

        public int PositionOf(string userId)
        {
            var sorted = GetAllSortedByRank();
            for (int i = 0; i < sorted.Count; i++)
                if (sorted[i].UserId == userId) return i + 1;
            return -1;
        }
    }
}
