using Rocket.API;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ApokPT.RocketPlugins
{
    public class JailTimeConfiguration : IRocketPluginConfiguration
    {

        public bool Enabled;
        public bool BanOnReconnect;
        public uint BanOnReconnectTime;
        public uint JailTimeInSeconds;
        public bool KillInsteadOfTeleport;
        public ulong WalkDistance;

        [XmlArrayItem(ElementName = "Cell")]
        public List<CellLoc> Cells;

        public IRocketPluginConfiguration DefaultConfiguration
        {
            get
            {
                JailTimeConfiguration config = new JailTimeConfiguration();
                config.Cells = new List<CellLoc>() {
                    new CellLoc("O'Leary 1", -240.5706,34.50486,16.71745),
                };
                config.KillInsteadOfTeleport = false;
                config.BanOnReconnect = false;
                config.BanOnReconnectTime = 0;
                config.JailTimeInSeconds = 600;
                config.WalkDistance = 5;
                Enabled = true;
                return config;

            }

        }

        public void LoadDefaults() { }
    }
}