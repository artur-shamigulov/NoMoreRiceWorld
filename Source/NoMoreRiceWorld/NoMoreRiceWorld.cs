using System.Diagnostics;
using HarmonyLib;
using UnityEngine;
using Verse;
using Verse.AI;

namespace NoMoreRiceWorld
{

    public class NoMoreRiceWorldSettings : ModSettings
    {
        public float VitaminsFallCoeff = 1f;
        public float ProteinsFallCoeff = 1f;
        public float CarbohydratesFallCoeff = 1f;
        public float FoodVarietyFallCoeff = 1f;
        public float FoodVarietyToleranceFallCoeff = 1f;
        
        public override void ExposeData()
        {
            Scribe_Values.Look(ref VitaminsFallCoeff, "vitaminsFallCoeff", 1f);
            Scribe_Values.Look(ref ProteinsFallCoeff, "proteinsFallCoeff", 1f);
            Scribe_Values.Look(ref CarbohydratesFallCoeff, "carbohydratesFallCoeff", 1f);
            Scribe_Values.Look(ref FoodVarietyFallCoeff, "foodVarietyFallCoeff", 1f);
            Scribe_Values.Look(ref FoodVarietyToleranceFallCoeff, "foodVarietyToleranceFallCoeff", 1f);
            base.ExposeData();
        }
    }
    
    public class NoMoreRiceWorldMod : Mod
    {
        private NoMoreRiceWorldSettings settings; 
        public NoMoreRiceWorldMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<NoMoreRiceWorldSettings>();
        }
        
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label($"Vitamins fall {settings.VitaminsFallCoeff.ToStringPercent()}");
            settings.VitaminsFallCoeff = listingStandard.Slider(settings.VitaminsFallCoeff, 0f, 10f);
            listingStandard.Label($"Proteins fall {settings.ProteinsFallCoeff.ToStringPercent()}");
            settings.ProteinsFallCoeff = listingStandard.Slider(settings.ProteinsFallCoeff, 0f, 10f);
            listingStandard.Label($"Carbohydrates fall {settings.CarbohydratesFallCoeff.ToStringPercent()}");
            settings.CarbohydratesFallCoeff = listingStandard.Slider(settings.CarbohydratesFallCoeff, 0f, 10f);
            listingStandard.Label($"Food variety fall {settings.FoodVarietyFallCoeff.ToStringPercent()}");
            settings.FoodVarietyFallCoeff = listingStandard.Slider(settings.FoodVarietyFallCoeff, 0f, 10f);
            listingStandard.Label($"Food variety tolerance fall {settings.FoodVarietyToleranceFallCoeff.ToStringPercent()}");
            settings.FoodVarietyToleranceFallCoeff = listingStandard.Slider(settings.FoodVarietyToleranceFallCoeff, 0f, 10f);
            listingStandard.Label($"Reload save to apply changes");
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }
        
        public override string SettingsCategory()
        {
            return "No more rice world";
        }
    }
}