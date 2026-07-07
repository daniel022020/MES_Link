using System;
using System.Collections.ObjectModel;
using MES_Link.Interfaces.MES_Link.Interfaces;
using MES_Link.MesSimulator;

namespace MES_Link.Tests
{
    public class FakeMesSimulatorService : IMesSimulatorService
    {
        public ObservableCollection<MesRouteBlock> Routes { get; } = new ObservableCollection<MesRouteBlock>();
        public string BaseUrl { get; set; } = "http://localhost:8080/";
        public bool IsRunning { get; private set; }
        public object RoutesLock { get; } = new object();
        public event Action<string> OnSimulatorLogAppended;

        public bool Start() { IsRunning = true; return true; }
        public bool Stop() { IsRunning = false; return true; }
    }
}