using System;
using System.Collections.ObjectModel;
using MES_Link.MesSimulator;

namespace MES_Link.Interfaces
{
    namespace MES_Link.Interfaces
    {
        public interface IMesSimulatorService
        {
            ObservableCollection<MesRouteBlock> Routes { get; }
            string BaseUrl { get; set; }
            bool IsRunning { get; }
            object RoutesLock { get; }
            bool Start();
            bool Stop();
            event Action<string> OnSimulatorLogAppended;
        }
    }
}