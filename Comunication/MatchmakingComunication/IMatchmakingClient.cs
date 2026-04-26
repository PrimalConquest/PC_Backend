using System;
using System.Collections.Generic;
using System.Text;

namespace MatchmakingComunication
{
    public interface IMatchmakingClient
    {
        Task QueueJoined(int position);
        Task QueueLeft();
        Task MatchFound(string serverIp, int serverPort);
        Task MatchmakingError(string message);
    }
}
