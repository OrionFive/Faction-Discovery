using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace FactionDiscovery.ScenarioParts
{
    public class ScenPart_KnownFactions : ScenPart
    {
        public int factionsFriendly = 1;
        public int factionsHostile = 1;
        public int factionCap = 12;

        private string bufFriendly, bufHostile, bufFactionCap;

        private FloatRange hostileRatio = new FloatRange(1/5f, 1f);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref factionsFriendly, "factionsFriendly", 1);
            Scribe_Values.Look(ref factionsHostile, "factionsHostile", 1);
            Scribe_Values.Look(ref factionCap, "factionCap", 12);
        }

        public override void DoEditInterface(Listing_ScenEdit listing)
        {
            Rect scenPartRect = listing.GetScenPartRect(this, RowHeight * 5f);
            Rect rect = scenPartRect.TopPartPixels(RowHeight);
            Text.Anchor = TextAnchor.LowerRight;
            Widgets.Label(rect.LeftHalf().Rounded(), "knownFriendlyFactions".Translate());
            Text.Anchor=TextAnchor.UpperLeft;
            Widgets.TextFieldNumeric(rect.RightHalf().Rounded(), ref factionsFriendly, ref bufFriendly);
            
            rect.y += RowHeight;
            Text.Anchor = TextAnchor.LowerRight;
            Widgets.Label(rect.LeftHalf().Rounded(), "knownHostileFactions".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.TextFieldNumeric(rect.RightHalf().Rounded(), ref factionsHostile, ref bufHostile, 1);

            
            rect.y += RowHeight;
            Text.Anchor = TextAnchor.LowerRight;
            Widgets.Label(rect.LeftHalf().Rounded(), "factionCap".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.TextFieldNumeric(rect.RightHalf().Rounded(), ref factionCap, ref bufFactionCap, 2, 50);

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.enabled = false;
            var hintRect = listing.GetRect(RowHeight*2);
            hintRect.y = rect.y+RowHeight;
            listing.GetRect(RowHeight*-2); // Gah, getting this to work -.-
            Widgets.TextArea(hintRect, "knownFactionsRecommendation".Translate());
            GUI.enabled = true;
        }

        public override string Summary(Scenario scen)
        {
            return "ScenPart_KnownFactions".Translate(factionsFriendly, factionsHostile);
        }

        public override void Randomize()
        {
            int total = Mathf.Max(2, Mathf.RoundToInt(Rand.Gaussian(1, 4.5f)));
            factionsHostile = Mathf.Max(1, Mathf.RoundToInt(total*hostileRatio.RandomInRange));
            factionsFriendly = Mathf.Max(0, total - factionsHostile);
            factionCap = Mathf.Max(2, Mathf.RoundToInt(Rand.Gaussian(11, 9))); // 2 - 20
        }

        public override bool CanCoexistWith(ScenPart other)
        {
            return !(other is ScenPart_KnownFactions);
        }

        public override bool TryMerge(ScenPart other)
        {
            if (other is ScenPart_KnownFactions knownFactions)
            {
                factionsFriendly =
                    Mathf.Max(Mathf.RoundToInt((factionsFriendly + knownFactions.factionsFriendly)/2f), 0);
                factionsHostile = Mathf.Max(
                    Mathf.RoundToInt((factionsHostile + knownFactions.factionsHostile)/2f), 1);
                return true;
            }
            return false;
        }

        public override void PostGameStart()
        {
            var knownFactions = Find.Scenario.AllParts.OfType<ScenPart_KnownFactions>().First();

            var friendly = knownFactions.factionsFriendly;
            var hostile = knownFactions.factionsHostile;
            var total = friendly + hostile;

            var existingHostile = MainUtilities.AllFactionsVisible.Count(f => f.HostileTo(Faction.OfPlayer) && !f.IsPlayer);
            var existingFriendly = MainUtilities.AllFactionsVisible.Count(f => !f.HostileTo(Faction.OfPlayer) && !f.IsPlayer);

            friendly -= existingFriendly;
            hostile -= existingHostile;

            // Now add factions according to minimum
            while (friendly > 0)
            {
                MainUtilities.CreateFaction(FactionRelationKind.Neutral, f => f.startingGoodwill.TrueMax >= 0, true);
                //Log.Message("Added friendly initial faction.");
                friendly--;
            }
            while (hostile > 0)
            {
                var cheap = !MainUtilities.AllFactions.Any(f => f.def.IsCheapAndHumanlike()); // Make sure there is at least 1 cheap faction
                MainUtilities.CreateFaction(FactionRelationKind.Hostile, f => f.startingGoodwill.TrueMin < -40 && (!cheap || f.IsCheapAndHumanlike()), true);
                //Log.Message("Added hostile initial faction.");
                hostile--;
            }

            
            bool instaDrop = Find.GameInitData.QuickStarted || Dropped;

            if(instaDrop)
                Find.LetterStack.ReceiveLetter("discoverAtDropLabel".Translate(), "discoverAtDropText".Translate(total), LetterDefOf.PositiveEvent);
            else
                Find.LetterStack.ReceiveLetter("discoverAtStartLabel".Translate(), "discoverAtStartText".Translate(total), LetterDefOf.PositiveEvent);
        }

        private static bool Dropped
        {
            get
            {
                var arriveMethod = Find.Scenario.AllParts.OfType<ScenPart_PlayerPawnsArriveMethod>().FirstOrDefault();
                if (arriveMethod == null) return false;
                return (PlayerPawnsArriveMethod)typeof(ScenPart_PlayerPawnsArriveMethod).GetField("method", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(arriveMethod)==PlayerPawnsArriveMethod.DropPods;
            }
        }
    }
}