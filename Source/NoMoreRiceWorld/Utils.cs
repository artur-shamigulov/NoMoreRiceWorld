using System.Diagnostics;
using Verse;
using RimWorld;

namespace NoMoreRiceWorld;

public static class Utils
{
    public static Dictionary<ElementsNeeds, float> GetNeedsAmount(Thing thing)
    {
        float vitCoeff = 0f;
        float carbCoeff = 0f;
        float protCoeff = 0f;

        Log.Message(FoodUtility.GetFoodKind(thing.def).ToString());
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
            else if (comp != null && comp.ingredients.Count > 1 )
            {
                foreach (ThingDef ingredient in comp.ingredients)
                {
                    if (ingredient.IsAnimalProduct)
                    {
                        protCoeff = 0.5f / comp.ingredients.Count();
                        vitCoeff = 0.5f / comp.ingredients.Count();
                    }
                    else if (ingredient.IsMeat)
                    {
                        protCoeff = 0.75f / comp.ingredients.Count();
                        carbCoeff = 0.25f / comp.ingredients.Count();
                    }
                    else if (ingredient.IsFungus)
                    {
                        protCoeff = 0.5f / comp.ingredients.Count();
                        carbCoeff = 0.5f / comp.ingredients.Count();
                    }
                    else if (FoodUtility.GetFoodKind(ingredient) == FoodKind.NonMeat)
                    {
                        carbCoeff = 1f / comp.ingredients.Count();
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

        return new Dictionary<ElementsNeeds, float>()
        {
            { VitaminesNeed.ElementsNeed, vitCoeff },
            { CarbohydratesNeed.ElementsNeed, carbCoeff },
            { ProteinsNeed.ElementsNeed, protCoeff },
        };
    }
}