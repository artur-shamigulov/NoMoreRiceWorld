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
        public bool ShowElementsNeeds;
        public bool ShowVarietyNeeds;
        public bool DisableFoodVarietyCooking;
        public bool AllowStackWithDiffIngredients;
        
        private DefMap<RimWorld.XenotypeDef, bool> xenotypeWithoutElementNeeds;

        public DefMap<RimWorld.XenotypeDef, bool> XenotypeWithoutElementNeeds
        {
            get
            {
                xenotypeWithoutElementNeeds ??= new DefMap<RimWorld.XenotypeDef, bool>();
                return xenotypeWithoutElementNeeds;
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref ShowElementsNeeds, "ShowElementsNeeds", false);
            Scribe_Values.Look(ref ShowVarietyNeeds, "ShowVarietyNeeds", false);
            Scribe_Values.Look(ref DisableFoodVarietyCooking, "ShowVarietyNeeds", false);
            Scribe_Values.Look(ref AllowStackWithDiffIngredients, "ShowVarietyNeeds", false);
            Scribe_Values.Look(ref VitaminsFallCoeff, "vitaminsFallCoeff", 1f);
            Scribe_Values.Look(ref ProteinsFallCoeff, "proteinsFallCoeff", 1f);
            Scribe_Values.Look(ref CarbohydratesFallCoeff, "carbohydratesFallCoeff", 1f);
            Scribe_Values.Look(ref FoodVarietyFallCoeff, "foodVarietyFallCoeff", 1f);
            Scribe_Values.Look(ref FoodVarietyToleranceFallCoeff, "foodVarietyToleranceFallCoeff", 1f);
            Scribe_Deep.Look<DefMap<RimWorld.XenotypeDef, bool>>(ref this.xenotypeWithoutElementNeeds, "ElementNeeds");
            base.ExposeData();
        }
    }
    
    public class NoMoreRiceWorldMod : Mod
    {
        private NoMoreRiceWorldSettings settings;
        private Vector2 scrollPosition;

        public bool DisableFoodVarietyCooking
        {
            get
            {
                if (settings == null)
                {
                    settings = GetSettings<NoMoreRiceWorldSettings>();
                }

                return settings.DisableFoodVarietyCooking;
            }
        }
        
        public bool AllowStackWithDiffIngredients
        {
            get
            {
                if (settings == null)
                {
                    settings = GetSettings<NoMoreRiceWorldSettings>();
                }

                return settings.AllowStackWithDiffIngredients;
            }
        }

        public NoMoreRiceWorldMod(ModContentPack content) : base(content)
        {
            scrollPosition = Vector2.zero;
        }
        
        public override void DoSettingsWindowContents(Rect inRect)
        {
            if (settings == null)
                settings = GetSettings<NoMoreRiceWorldSettings>();
            
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Always show elements needs", ref settings.ShowElementsNeeds, 0);
            listingStandard.CheckboxLabeled("Always show variety need", ref settings.ShowVarietyNeeds, 0);
            listingStandard.CheckboxLabeled("Allow stacking food with different ingredients", ref settings.AllowStackWithDiffIngredients, 0);
            listingStandard.CheckboxLabeled("Disable food variety cooking", ref settings.DisableFoodVarietyCooking, 0);
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
            if (ModsConfig.BiotechActive)
            {
                float height = 0f;
                listingStandard.GapLine(4f);
                listingStandard.Label($"Elements needs will be disable for selected xenotypes:");
                Rect outRect = listingStandard.GetRect(100f);
                
                foreach (var xenotypeDef in DefDatabase<RimWorld.XenotypeDef>.AllDefs)
                {
                    height += Text.CalcHeight(xenotypeDef.LabelCap.ToString(), outRect.width - 16f);
                }
                
                Rect rect2 = new Rect(
                    0f,
                    0f,
                    outRect.width - 16f,
                    height + 50f);
                Widgets.BeginScrollView(outRect, ref scrollPosition, rect2);
                Listing_Standard xenosTypesLS = new Listing_Standard(
                    inRect, (Func<Vector2>) (() => scrollPosition));
                xenosTypesLS.Begin(rect2);
                xenosTypesLS.ColumnWidth = rect2.width - 17f;
                xenosTypesLS.Gap(20f);
                foreach (var xenotypeDef in DefDatabase<RimWorld.XenotypeDef>.AllDefs)
                {
                    bool v = settings.XenotypeWithoutElementNeeds[xenotypeDef];
                    xenosTypesLS.CheckboxLabeled(xenotypeDef.LabelCap.ToString(), ref v, 0);
                    settings.XenotypeWithoutElementNeeds[xenotypeDef] = v;
                }
                xenosTypesLS.End();
                Widgets.EndScrollView();
            }
            listingStandard.GapLine(16f);
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