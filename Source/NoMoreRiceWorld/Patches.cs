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
        Log.Message($"Ate {__instance.def.defName} by {ingester.def.defName.ToString()} for {__result}");
        Dictionary<ElementsNeeds, float> coeffs = Utils.GetNeedsAmount(__instance);
        foreach (KeyValuePair<ElementsNeeds,float> keyValuePair in coeffs)
        {
            Log.Message($"{keyValuePair.Key.ToString()} {keyValuePair.Value}");
        }

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