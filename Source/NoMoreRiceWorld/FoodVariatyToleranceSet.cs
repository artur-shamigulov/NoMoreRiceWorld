using HarmonyLib;
using Verse;
using RimWorld;
using System.Text;

namespace NoMoreRiceWorld;

public class FoodVariatyToleranceSet: IExposable
{
    
    private DefMap<ThingDef, float> tolerances = new DefMap<ThingDef, float>();
    
    public void ExposeData()
    {
        Scribe_Deep.Look<DefMap<ThingDef, float>>(ref this.tolerances, "tolerances");
    }

    public float GetCurrentTolerance(Thing food)
    {
        float coeff = 0f;
        List<ThingDef> ingredients = new List<ThingDef>(); 
        CompIngredients comp = food.TryGetComp<CompIngredients>();
        if (comp != null && comp.ingredients.Count > 1)
        {
            comp.ingredients.ForEach((ThingDef x) => ingredients.Add(x));
        }
        else
        {
            ingredients.Add(food.def);
        }

        foreach (ThingDef thingDef in ingredients)
        {
            coeff += tolerances[thingDef] / ingredients.Count;
        }

        return coeff;
    }

    public float ConsumeFood(Thing food)
    {
        float coeff = 1f;
        List<ThingDef> ingredients = new List<ThingDef>(); 
        CompIngredients comp = food.TryGetComp<CompIngredients>();
        if (comp != null && comp.ingredients.Count > 1)
        {
            comp.ingredients.ForEach((ThingDef x) => ingredients.Add(x));
        }
        else
        {
            ingredients.Add(food.def);
        }

        foreach (ThingDef foodDef in ingredients)
        {
            coeff -= tolerances[foodDef] / ingredients.Count();
            tolerances[foodDef] = Math.Min(tolerances[foodDef] + 0.5f, 1f);
        }
        
        return coeff;
    }

    public string TolerancesString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (KeyValuePair<ThingDef,float> tolerance in tolerances)
        {
            if (tolerance.Value > 0)
            {
                if (stringBuilder.Length == 0)
                    stringBuilder.AppendLine("Disinterested in:");
                stringBuilder.AppendLine("   " + tolerance.Key.LabelCap + ": " + tolerance.Value.ToStringPercent());
            }
        }
        return stringBuilder.ToString().TrimEndNewlines();
    }

    public void NeedInterval()
    {
        foreach (KeyValuePair<ThingDef,float> tolerance in tolerances)
        {
            if (tolerance.Value > 0)
            {
                tolerances[tolerance.Key] = Math.Max(0f, tolerance.Value - 150f/240000f);
            }
        }
    }
}