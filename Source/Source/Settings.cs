using System;
using HugsLib.Settings;
using Verse;

namespace FactionDiscovery
{
    internal class Settings
    {
        public static SettingHandle<float> newFactionSettlementFactor;
        public static SettingHandle<int> minSettlements;

        public static SettingHandle<int> minOfAnyFaction;
        public static SettingHandle<int> maxOfAnyFaction;

        //public static SettingHandle<bool> removeDuplicateFactions;

        public static void Initialize(ModSettingsPack settings)
        {
            newFactionSettlementFactor = settings.GetHandle("newFactionSettlementFactor", "SettingSettlementFactor".Translate(), "SettingSettlementFactorDesc".Translate(), 0.7f, AtLeast(() => 0));
            minSettlements = settings.GetHandle("minSettlements", "SettingSettlementsMin".Translate(), "SettingSettlementsMinDesc".Translate(), 3, AtLeast(() => 0));
            minOfAnyFaction = settings.GetHandle("minOfAnyFaction", "SettingMinOfAnyFaction".Translate(), "SettingMinOfAnyFactionDesc".Translate(), 1, Validators.IntRangeValidator(0, 5));
            maxOfAnyFaction = settings.GetHandle("maxOfAnyFaction", "SettingMaxOfAnyFaction".Translate(), "SettingMaxOfAnyFactionDesc".Translate(), 1, Validators.IntRangeValidator(1, 10));
            //removeDuplicateFactions = settings.GetHandle("removeDuplicateFactions", "SettingRemoveDuplicates".Translate(), "SettingRemoveDuplicatesDesc".Translate(), false);
            //removeDuplicateFactions.OnValueChanged += OnChangedRemoveDuplicates;
        }

        //private void OnChangedRemoveDuplicates(bool value)
        //{
        //    if (value)
        //    {
        //        maxOfAnyFaction.Value = 1;
        //        minOfAnyFaction.Value = Mathf.Min(minOfAnyFaction.Value, 1);
        //    }
        //    else
        //    {
        //        maxOfAnyFaction.Value = 2;
        //    }
        //}

        private static SettingHandle.ValueIsValid AtLeast(Func<float> amount)
        {
            return value => float.TryParse(value, out var actual) && actual >= amount();
        }
    }
}