using Rocket.API;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ApokPT.RocketPlugins
{
    public class JailTimeConfiguration : IRocketPluginConfiguration
    { 
        public bool BanOnReconnect;
        public uint BanOnReconnectTime;
        public uint JailTimeInSeconds;
        public bool KillInsteadOfTeleport;
        public ulong WalkDistance;
        public bool Enabled;

        //Warning incase plugin is loaded using another map that is not Washington
        public string tutorial;
        public string tutorial2;
        public string tutorial3;
        public string tutorial4;

        [XmlArray("Cells"), XmlArrayItem(ElementName = "Cell")]
        public List<CellLoc> Cells;

        public void LoadDefaults()
        {

            BanOnReconnect = false;
            BanOnReconnectTime = 0;
            JailTimeInSeconds = 600;
            KillInsteadOfTeleport = false;
            WalkDistance = 5;
            Enabled = true;

            tutorial = "Default cell is for map WASHINGTON, on top of the Seattle tower.";
            tutorial2 = "DO NOT USE -Tower- if not using Washington map";
            tutorial3 = "Player maybe in a place unknown.";
            tutorial4 = "-Tower- is safe to remove after establishing a known cell.";

            Cells = new List<CellLoc>
            {
                new CellLoc("Tower", -274.423828125, 93.1066665649414, 44.617992401123047)

            };

        }
    }
}