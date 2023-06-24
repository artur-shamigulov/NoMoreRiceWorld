using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using System.Collections;
using RimWorld.Planet;

namespace NoMoreRiceWorld;

[StaticConstructorOnStartup]
public static class NoMoreRiceWorld
{
    static NoMoreRiceWorld()
    {
        var harmony = new Harmony("com.voidfirefly.nomorericeworld");
        harmony.PatchAll();
        LoadedModManager.GetMod<NoMoreRiceWorldMod>(
        ).GetSettings<NoMoreRiceWorldSettings>();
        
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
    static void Postfix(ref bool __result, Pawn ___pawn, NeedDef nd)
    {
        if (nd.defName == "FoodVariaty")
        {
            if (__result)
            {
                if (___pawn.needs?.joy == null)
                {
                    __result = false;
                    return;
                }
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
        else if (ModsConfig.BiotechActive && nd.needClass.IsSubclassOf(typeof(BaseFoodNeed)))
        {
            var elementsNeed = LoadedModManager.GetMod<NoMoreRiceWorldMod>(
            ).GetSettings<NoMoreRiceWorldSettings>().XenotypeWithoutElementNeeds;
            if (___pawn?.genes?.Xenotype != null  && elementsNeed[___pawn.genes.Xenotype])
            {
                __result = false;
            }
        }
    }
}

[HarmonyPatch(typeof(CompIngredients))]
[HarmonyPatch(nameof(CompIngredients.AllowStackWith))]
class AllowStackWithPatch
{
    static void Postfix(CompIngredients __instance, ref bool __result, Thing otherStack)
    {
        if (__result == false)
        {
            return;
        }
        if (LoadedModManager.GetMod<NoMoreRiceWorldMod>().AllowStackWithDiffIngredients)
        {
            return;
        }
        CompIngredients otherComp = otherStack.TryGetComp<CompIngredients>();
        if (otherComp == null)
        {
            return;
        }

        if (__instance.ingredients.Count != otherComp.ingredients.Count)
        {
            __result = false;
            return;
        }

        foreach (ThingDef ingredient in __instance.ingredients)
        {
            if (!otherComp.ingredients.Contains(ingredient))
            {
                __result = false;
                return;
            }
        }
    }
}

[HarmonyPatch(typeof(WorkGiver_DoBill))]
[HarmonyPatch("TryFindBestBillIngredientsInSet_AllowMix")]
class TryFindBestBillIngredientsInSet_AllowMixPatch
{
    static void Postfix(WorkGiver_DoBill __instance, ref bool __result, List<Thing> availableThings, Bill bill, List<ThingCount> chosen)
    {
        if (__result == false)
        {
            return;
        }
        if (IngredientsUsedComponent.UsedIngredients == null)
        {
            return;
        }
        if (LoadedModManager.GetMod<NoMoreRiceWorldMod>().DisableFoodVarietyCooking)
        {
            return;
        }
        //Log.Message("---->TryFindBestBillIngredientsInSet_AllowMix");
        ThingDefCountClass thingDefCountClass = bill.recipe?.products?.First();
        //Log.Message($"IS thingDefCountClass null - {thingDefCountClass.thingDef.defName} {thingDefCountClass.thingDef.IsNutritionGivingIngestible}");
        if (thingDefCountClass != null && thingDefCountClass.thingDef.IsNutritionGivingIngestible)
        {
            //Log.Message("---->Try search variants");
            //Log.Message("Before "+string.Join(",", availableThings.Select(x => x.def.defName.ToString())));
            availableThings.SortBy<Thing, int>( x => IngredientsUsedComponent.CountInQueue(x.def));
            
            List<Thing> localAvailableThings = availableThings.FindAll(
                x =>
                    chosen.Any(y => (y.Thing.Position - x.Position).LengthHorizontal < 20));
            //Log.Message("After "+string.Join(",", availableThings.Select(x => x.def.defName.ToString())));
            chosen.Clear();
            foreach (IngredientCount ingredient in bill.recipe.ingredients)
            {
                float baseCount = ingredient.GetBaseCount();
                IngredientVariants variant = new IngredientVariants();
                foreach(Thing availableThing in localAvailableThings)
                {
                    //Log.Message($"Used times: {IngredientsUsedComponent.CountInQueue(availableThing.def)}");
                    if (ingredient.filter.Allows(availableThing) &&
                        (ingredient.IsFixedIngredient || bill.ingredientFilter.Allows(availableThing)))
                    {
                        //Log.Message("Add first ingredient");
                        variant.AddIngredient(
                            availableThing, bill.recipe.IngredientValueGetter, baseCount);
                        if (baseCount > variant.Amount)
                        {
                            //Log.Message("Not enough try to find others ingredients");
                            for(int index = localAvailableThings.IndexOf(availableThing) + 1; index < localAvailableThings.Count; index++)
                            {
                                Thing otherThing = localAvailableThings[index];
                                if (ingredient.filter.Allows(otherThing) &&
                                    (ingredient.IsFixedIngredient || bill.ingredientFilter.Allows(otherThing)))
                                {
                                    //Log.Message($"Add {index} ingredient");
                                    variant.AddIngredient(
                                        otherThing, bill.recipe.IngredientValueGetter, baseCount - variant.Amount);
                                    if (baseCount <= variant.Amount)
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                        if (baseCount <= variant.Amount)
                        {
                            chosen.AddRange(variant.Ingredients);
                            //Log.Message("Variant" + string.Join("; ", variant.Ingredients.Select(x => x.Thing.def.defName + "-" + x.Count)));
                            break;
                        }
                        //else
                        //{
                            //Log.Message("Not variant" + string.Join("; ", variant.Ingredients.Select(x => x.Thing.def.defName + "-" + x.Count)));
                        //}
                    }
                }
            }
            return;
        }
        //Log.Message(string.Join(",", chosen.Select(x => x.Thing.def.defName.ToString())));
        //Log.Message(string.Join(",", availableThings.Select(x => x.def.defName.ToString())));
        //Log.Message("<----TryFindBestBillIngredientsInSet_AllowMix");
    }
}

[HarmonyPatch(typeof(CompIngredients))]
[HarmonyPatch(nameof(CompIngredients.RegisterIngredient))]
class RegisterIngredientPatch
{
    static public void Postfix(ThingDef def)
    {
        Log.Message($"CountInQueue: {def.defName}");
        if (IngredientsUsedComponent.UsedIngredients[def] == null)
        {
            IngredientsUsedComponent.UsedIngredients[def] = new IngredientsUsedComponentQueue();
        }
        IngredientsUsedComponent.UsedIngredients[def].componentQueue.Enqueue(Find.TickManager.TicksGame);
        Log.Message($"CountInQueue: {IngredientsUsedComponent.CountInQueue(def)}");
    }
}