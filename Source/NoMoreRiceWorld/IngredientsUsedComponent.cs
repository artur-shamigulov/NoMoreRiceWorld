using RimWorld.Planet;
using Verse;
using System.Linq;

namespace NoMoreRiceWorld;


public class IngredientsUsedComponentQueue : IExposable
{
    public Queue<int> componentQueue;
    private List<int> componentList;

    public IngredientsUsedComponentQueue()
    {
        componentQueue = new Queue<int>();
        componentList = new List<int>();
        //Log.Message("IngredientsUsedComponentQueue inited");
    }
    
    public void ExposeData()
    {
        if (componentQueue.Count > 0)
        {
            componentList.Clear();
            foreach (int value in componentQueue)
            {
                componentList.Append(value);
            }
        }
        
        Scribe_Collections.Look(ref componentList, "IngredientsUsedComponentList", LookMode.Value);
        if (componentList.Count > 0)
        {
            componentQueue.Clear();
            componentList.ForEach(x => componentQueue.Enqueue(x));
        }
        //Log.Message("IngredientsUsedComponentQueue" + string.Join(", ", componentList.Select(x => x)));
    }
}

public class IngredientsUsedComponent: WorldComponent
{
    public static DefMap<ThingDef, IngredientsUsedComponentQueue> UsedIngredients
    {
        get
        {
            if (usedIngredients == null)
            {
                usedIngredients = new DefMap<ThingDef, IngredientsUsedComponentQueue>();
            }
            return usedIngredients;
        }
    }
    
    private static DefMap<ThingDef, IngredientsUsedComponentQueue> usedIngredients;
    
    public IngredientsUsedComponent(World world) : base(world)
    {
        usedIngredients = new DefMap<ThingDef, IngredientsUsedComponentQueue>();
    }
    
    static public int CountInQueue(ThingDef def)
    {
        if (UsedIngredients[def] == null)
        {
            return 0;
        }

        while (UsedIngredients[def].componentQueue.Count > 0)
        {
            if (UsedIngredients[def].componentQueue.Peek() + 60000 < Find.TickManager.TicksGame)
            {
                UsedIngredients[def].componentQueue.Dequeue();
            }
            else
            {
                break;
            }
        }
        
        return UsedIngredients[def].componentQueue.Count;
    }
    
    public override void ExposeData()
    {
        Scribe_Deep.Look<DefMap<ThingDef, IngredientsUsedComponentQueue>>(ref usedIngredients, "ingredientsUsedComponent");
    }
}