using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MatchmakingComunication
{
    public interface IMatchmakingHub
    {
        public Task JoinQueue();
        public Task LeaveQueue();
    }
}
