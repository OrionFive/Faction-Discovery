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
            Log.Message("Faction Discovery is out of commission. Please use Vanilla Factions Expanded - Core instead.");
            //LongEventHandler.QueueLongEvent(MainUtilities.CheckFactions, "CheckFactions", false, null);
        }
    }
}
