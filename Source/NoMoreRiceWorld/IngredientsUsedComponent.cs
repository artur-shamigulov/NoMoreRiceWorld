using RimWorld.Planet;
using Verse;

namespace NoMoreRiceWorld;

public class IngredientsUsedComponent: WorldComponent
{
    public static DefMap<ThingDef, Queue<int>> UsedIngredients
    {
        get
        {
            if (usedIngredients == null)
            {
                usedIngredients = new DefMap<ThingDef, Queue<int>>();
            }
            return usedIngredients;
        }
    }
    
    private static DefMap<ThingDef, Queue<int>> usedIngredients;
    
    public IngredientsUsedComponent(World world) : base(world)
    {
        usedIngredients = new DefMap<ThingDef, Queue<int>>();
    }
}