using SimulationEngine.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace BattleComunication.Interfaces
{
    public interface ISimulationClient
    {
        bool RecieveCommand(ICommandInfo info);
        void ReceiveGameSetup();
    }
}
