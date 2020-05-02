using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace FactionDiscovery
{
    internal static class MainUtilities
    {
        private static readonly FloatRange settlementsPer100KTiles = new FloatRange(75f, 85f);

        public static void CheckFactions()
        {
            if (Current.Game == null) return;

            EnsureAllFactionsArePresent();
        }

        public static Faction CreateFaction(FactionRelationKind kind, Func<FactionDef, bool> qualifier, bool isGameStart)
        {
            //Log.Message("Creating faction...");
            var factionDefs = (from fa in DefDatabase<FactionDef>.AllDefs
                where fa.canMakeRandomly && qualifier(fa)
                    && (!isGameStart || AllFactions.Count(f => f.def == fa) < fa.maxCountAtGameStart)
                select fa).ToArray();
            var facDef = !AllFactions.Any()
                ? factionDefs.RandomElement()
                : factionDefs.RandomElementByWeight(FactionChance);
            Faction faction = FactionGenerator.NewGeneratedFaction(facDef);

            InitializeFaction(faction, kind);
            return faction;
        }

        private static void InitializeFaction(Faction faction, FactionRelationKind kind)
        {
            faction.TrySetRelationKind(Faction.OfPlayer, kind, false);
            Find.FactionManager.Add(faction);
        }

        private static float FactionChance(FactionDef def)
        {
            var existing = AllFactionsVisible.Count(fa => fa.def == def);
            var max = AllFactionsVisible.Count()/2;
            var percent = Mathf.InverseLerp(max, 0, existing);
            //Log.Message(def.LabelCap+": "+existing+" = "+percent);
            return percent < 0.1f ? 0.1f : percent;
        }

        public static int ManhattanDistanceFlat(IntVec2 a, IntVec2 b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.z - b.z);
        }

        public static bool IsCheapAndHumanlike(this FactionDef def)
        {
            return def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat) <= 35 && def.humanlikeFaction;
        }

        public static void EnsureAllFactionsArePresent()
        {
            foreach (FactionDef current in DefDatabase<FactionDef>.AllDefs)
            {
                //if (!current.hidden) continue;

                if (current.isPlayer) continue;

                var count = Find.FactionManager.AllFactions.Count(f => f.def == current);

                if (count >= Settings.minOfAnyFaction) continue;
                if (count >= Settings.maxOfAnyFaction) continue;

                int amount = current.requiredCountAtGameStart + (current.canMakeRandomly ? Rand.RangeInclusive(0, 1) : 0);
                amount = Mathf.Clamp(amount, Settings.minOfAnyFaction, Settings.maxOfAnyFaction);

                for (int j = count; j < amount; j++)
                {
                    Faction faction = FactionGenerator.NewGeneratedFaction(current);
                    var relationKind = GetFactionKind(faction, j == 0);
                    InitializeFaction(faction, relationKind);
                    Log.Message("Created faction for " + faction.def.LabelCap);

                    if (!faction.def.hidden)
                    {
                        CreateSettlements(faction);
                    }
                }
            }
        }

        private static FactionRelationKind GetFactionKind(Faction faction, bool firstOfKind)
        {
            var result = FactionRelationKind.Hostile;
            if (faction.def.CanEverBeNonHostile)
            {
                if (!firstOfKind || !faction.def.mustStartOneEnemy)
                {
                    if (faction.def.startingGoodwill.RandomInRange > 0) result = FactionRelationKind.Neutral;
                }
            }
            return result;
        }

        private static void CreateSettlements(Faction faction)
        {
            int existingFactions = Find.FactionManager.AllFactionsVisible.Count();
            int amount = GenMath.RoundRandom(Find.WorldGrid.TilesCount / 100000f * settlementsPer100KTiles.RandomInRange / existingFactions * Settings.newFactionSettlementFactor); // New factions get less bases
            amount = Mathf.Max(Settings.minSettlements, amount);

            int count = 0;
            for (int k = 0; k < amount; k++)
            {
                var tile = TileFinder.RandomSettlementTileFor(faction);
                if (tile == 0) continue;

                var factionBase = (Settlement) WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                factionBase.SetFaction(faction);
                factionBase.Tile = tile;
                factionBase.Name = SettlementNameGenerator.GenerateSettlementName(factionBase);
                Find.WorldObjects.Add(factionBase);
                count++;
            }
            Log.Message("Created " + count + " settlements for " + faction.def.LabelCap);
        }

        public static IEnumerable<Faction> AllFactionsVisible {get
        {
            return Find.FactionManager.AllFactions.Where(f => !f.def.hidden && f != Faction.OfPlayer);
        }}
        public static IEnumerable<Faction> AllFactions {get
        {
            return Find.FactionManager.AllFactions.Where(f => f != Faction.OfPlayer);
        }}
        public static List<ScenPart> KnownFactionsScenPart
        {
            get
            {
                var parts =
                    (List<ScenPart>)
                        typeof(Scenario).GetField("parts", BindingFlags.Instance | BindingFlags.NonPublic)
                            .GetValue(Find.Scenario);
                return parts;
            }
        }
    }
}
