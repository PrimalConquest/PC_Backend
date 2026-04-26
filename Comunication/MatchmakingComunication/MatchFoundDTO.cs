using CommunicationShared;

namespace MatchmakingComunication
{
    public class MatchFoundDTO : DTO<MatchFoundDTO>
    {
        public string Ip { get; set; } = "";
        public int Port { get; set; } = 0;
    }
}
