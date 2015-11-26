//Original Author: ApokPT - https://github.com/ApokPT
using Rocket.API;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ApokPT.RocketPlugins
{
    public class JailTimeConfiguration : IRocketPluginConfiguration
    {
        [XmlArray("Cells"), XmlArrayItem(ElementName = "Cell")]
        public List<CellLoc> Cells = new List<CellLoc>();

        public bool ShowWarnings;
        public bool BanOnReconnect = false;
        public uint BanOnReconnectTime = 0;
        public uint JailTimeInSeconds = 600;
        public bool KillInsteadOfTeleport = false;
        public ulong WalkDistance = 5;
        public bool Enabled = true;  
        public void LoadDefaults() { }
    }
}