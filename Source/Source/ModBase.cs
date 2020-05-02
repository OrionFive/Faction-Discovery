using Verse;

namespace FactionDiscovery
{
    internal class ModBase : HugsLib.ModBase
    {
        public override string ModIdentifier => "FactionDiscovery";

        public override void DefsLoaded()
        {
            FactionDiscovery.Settings.Initialize(Settings);
        }

        public override void WorldLoaded()
        {
            RunCheck();
        }

        public override void SettingsChanged()
        {
            RunCheck();
        }

        private static void RunCheck()
        {
            LongEventHandler.QueueLongEvent(MainUtilities.CheckFactions, "CheckFactions", false, null);
        }
    }
}
