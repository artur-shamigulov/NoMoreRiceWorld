using RimWorld;
using Verse;
using HarmonyLib;

namespace NoMoreRiceWorld;

[StaticConstructorOnStartup]
public static class NoMoreRiceWorld
{
    static NoMoreRiceWorld()
    {
        var harmony = new Harmony("com.voidfirefly.nomorericeworld");
        harmony.PatchAll();
        
        try
        {
            ((Action)(() =>
            {
                if (LoadedModManager.RunningModsListForReading.Any(x=> x.Name == "Vanilla Nutrient Paste Expanded"))
                {
                    harmony.Patch(AccessTools.Method(typeof(Utils), nameof(Utils.GetNeedsAmount)),
                        postfix: new HarmonyMethod(typeof(GetNeedsAmountVNPEPatch), nameof(GetNeedsAmountVNPEPatch.Postfix)));
                }
            }))();
        }
        catch (TypeLoadException) { }
    }
}

static class GetNeedsAmountVNPEPatch
{
    public static void Postfix(ref Dictionary<ElementsNeeds, float> __result, Thing thing)
    {
        VNPE.Building_NutrientPasteTap nutrientPasteDispenser = thing as VNPE.Building_NutrientPasteTap;
        List<ThingDef> ingredients = new List<ThingDef>();
        if (nutrientPasteDispenser != null)
        {
            var net = nutrientPasteDispenser.resourceComp.PipeNet;
            for (int i = 0; i < net.storages.Count; i++)
            {
                var parent = net.storages[i].parent;
                if (parent.TryGetComp<VNPE.CompRegisterIngredients>() is VNPE.CompRegisterIngredients storageIngredients)
                {
                    for (int o = 0; o < storageIngredients.ingredients.Count; o++)
                        ingredients.Add(storageIngredients.ingredients[o]);
                }
            }
        }
        
        foreach (ThingDef ingredient in ingredients)
        {
            
            if (ingredient.HasModExtension<ElementsDefModExtension>())
            {
                ElementsDefModExtension elements = ingredient.GetModExtension<ElementsDefModExtension>();
                __result[ElementsNeeds.Vitamines] += elements.Vitamines / ingredients.Count();
                __result[ElementsNeeds.Carbohydrates] += elements.Carbohydrates / ingredients.Count();
                __result[ElementsNeeds.Proteins] += elements.Proteins / ingredients.Count();
            }
            else if (ingredient.IsAnimalProduct)
            {
                __result[ElementsNeeds.Proteins] += 0.5f / ingredients.Count();
                __result[ElementsNeeds.Vitamines] += 0.5f / ingredients.Count();
            }
            else if (ingredient.IsMeat)
            {
                __result[ElementsNeeds.Proteins] += 0.75f / ingredients.Count();
                __result[ElementsNeeds.Carbohydrates] += 0.25f / ingredients.Count();
            }
            else if (ingredient.IsFungus)
            {
                __result[ElementsNeeds.Proteins] += 0.5f / ingredients.Count();
                __result[ElementsNeeds.Carbohydrates] += 0.5f / ingredients.Count();
            }
            else if (FoodUtility.GetFoodKind(ingredient) == FoodKind.NonMeat)
            {
                __result[ElementsNeeds.Carbohydrates] += 1f / ingredients.Count();
            }
        }
    }
}

[HarmonyPatch(typeof(FoodUtility))]
[HarmonyPatch(nameof(FoodUtility.FoodOptimality))]
static class FoodOptimalityPatch
{
    static void Postfix(
        ref float __result,
        Pawn eater,
        Thing foodSource,
        ThingDef foodDef,
        float dist,
        bool takingToInventory = false)
    {
        if (eater == null || foodSource == null)
            return;
        VitaminesNeed vitNeed = eater.needs.TryGetNeed<VitaminesNeed>();
        ProteinsNeed protNeed = eater.needs.TryGetNeed<ProteinsNeed>();
        CarbohydratesNeed carbNeed = eater.needs.TryGetNeed<CarbohydratesNeed>();

        if (vitNeed == null || protNeed == null || carbNeed == null)
        {
            return;
        }

        Dictionary<ElementsNeeds, float> coeffs = Utils.GetNeedsAmount(foodSource);
        List<BaseFoodNeed> needs = new List<BaseFoodNeed>()
        {
            vitNeed, protNeed, carbNeed
        };
        float currentСontentment = 0f;

        needs.ForEach(x =>
        {
            switch (x.CurLevel)
            {
                case < 0.25f:
                    currentСontentment += coeffs[x.GetElementNeed] * 40f;
                    break;
                case < 0.75f:
                    currentСontentment += coeffs[x.GetElementNeed] * 20f;
                    break;
            }
        });
        
        __result += currentСontentment;

        FoodVariatyNeed nd = eater.needs.TryGetNeed<FoodVariatyNeed>();
        if (nd != null)
        {
            __result -= nd.Tolerance.GetCurrentTolerance(foodSource) * 40f;
        }
    }
}

[HarmonyPatch(typeof(Thing))]
[HarmonyPatch(nameof(Thing.Ingested))]
static class ThingsIngestedPatch
{
    static void Postfix(Thing __instance, float __result, ref Pawn ingester, ref float nutritionWanted)
    {
        if (__instance == null || ingester == null)
        {
            return;
        }
        Dictionary<ElementsNeeds, float> coeffs = Utils.GetNeedsAmount(__instance);
        ingester.needs.TryGetNeed<VitaminesNeed>()?.ConsumeAmount(__result * coeffs[VitaminesNeed.ElementsNeed]);
        ingester.needs.TryGetNeed<ProteinsNeed>()?.ConsumeAmount(__result * coeffs[ProteinsNeed.ElementsNeed]);
        ingester.needs.TryGetNeed<CarbohydratesNeed>()?.ConsumeAmount(__result * coeffs[CarbohydratesNeed.ElementsNeed]);
        ingester.needs.TryGetNeed<FoodVariatyNeed>()?.ConsumeFood(__instance, __result);
    }
}

[HarmonyPatch(typeof(Pawn_NeedsTracker))]
[HarmonyPatch("ShouldHaveNeed")]
class ShouldHaveNeedPatch
{
    static void Postfix(bool __result, Pawn ___pawn, NeedDef nd)
    {
        if(nd.defName != "FoodVariaty")
            return;
        if (__result)
        {
            List<Trait> allTraits = ___pawn.story?.traits?.allTraits;
            if (allTraits != null)
            {
                if (!allTraits.NullOrEmpty<Trait>())
                {
                    foreach (Trait trait in allTraits)
                    {
                        if (trait.def.defName == "Ascetic" && !trait.Suppressed)
                        {
                            __result = false;
                        }
                    }
                }
            }

            if (___pawn.Ideo != null && ___pawn.Ideo.IdeoCausesHumanMeatCravings())
            {
                __result = false;
            }
        }
    }
}