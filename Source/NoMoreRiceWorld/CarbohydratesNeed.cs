using Verse;

namespace NoMoreRiceWorld;

public class CarbohydratesNeed: BaseFoodNeed
{
    public new static ElementsNeeds ElementsNeed = ElementsNeeds.Carbohydrates;
    public override ElementsNeeds GetElementNeed => ElementsNeed;

    public override string FullDefName()
    {
        return "CarbohydratesFull";
    }

    public override string LackDefName()
    {
        return "CarbohydratesLack";
    }

    public CarbohydratesNeed(Pawn pawn)
        : base(pawn)
    {
        CoeffFromSetting = LoadedModManager.GetMod<NoMoreRiceWorldMod>(
            ).GetSettings<NoMoreRiceWorldSettings>().CarbohydratesFallCoeff;
    }
}