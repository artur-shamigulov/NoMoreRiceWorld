using System.Diagnostics;
using Verse;
using RimWorld;
using Debug = UnityEngine.Debug;

namespace NoMoreRiceWorld;

public static class Utils
{
    public static Dictionary<ElementsNeeds, float> GetNeedsAmount(Thing thing)
    {
        float vitCoeff = 0f;
        float carbCoeff = 0f;
        float protCoeff = 0f;

        if (thing.def.IsNutritionGivingIngestible)
        {
            CompIngredients comp = thing.TryGetComp<CompIngredients>();
            if (FoodUtility.IsHumanlikeCorpseOrHumanlikeMeat(thing, thing.def))
            {
                vitCoeff = 0.33f;
                carbCoeff = 0.33f;
                protCoeff = 0.33f;
            }
            else if (thing.def.IsCorpse)
            {
                protCoeff = 0.75f;
                carbCoeff = 0.25f;
            }
            else if (thing.def.HasModExtension<ElementsDefModExtension>())
            {
                ElementsDefModExtension elements = thing.def.GetModExtension<ElementsDefModExtension>();
                vitCoeff = elements.Vitamines;
                carbCoeff = elements.Carbohydrates;
                protCoeff = elements.Proteins;
            }
            else if (comp != null)
            {
                foreach (ThingDef ingredient in comp.ingredients)
                {
                    if (ingredient.HasModExtension<ElementsDefModExtension>())
                    {
                        ElementsDefModExtension elements = ingredient.GetModExtension<ElementsDefModExtension>();
                        vitCoeff += elements.Vitamines / comp.ingredients.Count();
                        carbCoeff += elements.Carbohydrates / comp.ingredients.Count();
                        protCoeff += elements.Proteins / comp.ingredients.Count();
                    }
                    else if (ingredient.IsAnimalProduct)
                    {
                        protCoeff += 0.5f / comp.ingredients.Count();
                        vitCoeff += 0.5f / comp.ingredients.Count();
                    }
                    else if (ingredient.IsMeat)
                    {
                        protCoeff += 0.75f / comp.ingredients.Count();
                        carbCoeff += 0.25f / comp.ingredients.Count();
                    }
                    else if (ingredient.IsFungus)
                    {
                        protCoeff += 0.5f / comp.ingredients.Count();
                        carbCoeff += 0.5f / comp.ingredients.Count();
                    }
                    else if (FoodUtility.GetFoodKind(ingredient) == FoodKind.Meat)
                    {
                        protCoeff += 0.75f / comp.ingredients.Count();
                        carbCoeff += 0.25f / comp.ingredients.Count();
                    }
                    else if (FoodUtility.GetFoodKind(ingredient) == FoodKind.NonMeat)
                    {
                        carbCoeff += 1f / comp.ingredients.Count();
                    }
                }
            }
            else if (thing.def.IsAnimalProduct)
            {
                protCoeff = 0.5f;
                vitCoeff = 0.5f;
            }
            else if (thing.def.IsMeat)
            {
                protCoeff = 0.75f;
                carbCoeff = 0.25f;
            }
            else if (thing.def.IsFungus)
            {
                protCoeff = 0.5f;
                carbCoeff = 0.5f;
            }
            else if (FoodUtility.GetFoodKind(thing) == FoodKind.Meat)
            {
                protCoeff = 0.75f;
                carbCoeff = 0.25f;
            }
            else if (FoodUtility.GetFoodKind(thing.def) == FoodKind.NonMeat)
            {
                carbCoeff = 1f;
            }
        }
        else
        {
            Building_NutrientPasteDispenser nutrientPasteDispenser = thing as Building_NutrientPasteDispenser;
            if (nutrientPasteDispenser != null)
            {
                Thing dispensable = nutrientPasteDispenser.FindFeedInAnyHopper();
                if (dispensable != null)
                {
                    if (dispensable.def.IsAnimalProduct)
                    {
                        protCoeff = 0.5f;
                        vitCoeff = 0.5f;
                    }
                    else if (dispensable.def.IsMeat)
                    {
                        protCoeff = 0.75f;
                        carbCoeff = 0.25f;
                    }
                    else if (dispensable.def.IsFungus)
                    {
                        protCoeff = 0.5f;
                        carbCoeff = 0.5f;
                    }
                    else if (FoodUtility.GetFoodKind(dispensable) == FoodKind.Meat)
                    {
                        protCoeff = 0.75f;
                        carbCoeff = 0.25f;
                    }
                    else if (FoodUtility.GetFoodKind(dispensable) == FoodKind.NonMeat)
                    {
                        carbCoeff = 1f;
                    }
                }
            }
        }

        return new Dictionary<ElementsNeeds, float>()
        {
            { VitaminesNeed.ElementsNeed, vitCoeff },
            { CarbohydratesNeed.ElementsNeed, carbCoeff },
            { ProteinsNeed.ElementsNeed, protCoeff },
        };
    }
}