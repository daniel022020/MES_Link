using System;

namespace MES_Link.IniParser
{
    public class IniConfig
    {
        public MesSimulatorSettingsSection MesSimulatorSettings { get; set; } = new MesSimulatorSettingsSection();
    }

    public class MesSimulatorSettingsSection
    {
        public string BaseUrl { get; set; } = string.Empty;
    }
}