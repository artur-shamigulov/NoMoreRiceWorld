using RimWorld;
using Verse;

namespace NoMoreRiceWorld;

public class ThoughtWorker_NeedFoodVariety : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        FoodVariatyNeed nd = p.needs.TryGetNeed<FoodVariatyNeed>();
        if (nd == null)
            return ThoughtState.Inactive;
        switch (nd.CurCategory)
        {
            case FoodVariatyCategory.Empty:
                return ThoughtState.ActiveAtStage(0);
            case FoodVariatyCategory.VeryLow:
                return ThoughtState.ActiveAtStage(1);
            case FoodVariatyCategory.Low:
                return ThoughtState.ActiveAtStage(2);
            case FoodVariatyCategory.Satisfied:
                return ThoughtState.Inactive;
            case FoodVariatyCategory.High:
                return ThoughtState.ActiveAtStage(3);
            case FoodVariatyCategory.Extreme:
                return ThoughtState.ActiveAtStage(4);
            default:
                throw new NotImplementedException();
        }
    }
}