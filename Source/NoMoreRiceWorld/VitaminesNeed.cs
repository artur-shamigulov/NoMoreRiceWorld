using Verse;

namespace NoMoreRiceWorld;

public class VitaminesNeed: BaseFoodNeed
{
    public new static ElementsNeeds ElementsNeed = ElementsNeeds.Vitamines;
    public override ElementsNeeds GetElementNeed => ElementsNeed;
    
    public override string FullDefName()
    {
        return "VitaminesFull";
    }

    public override string LackDefName()
    {
        return "VitaminesLack";
    }
    
    public VitaminesNeed(Pawn pawn)
        : base(pawn)
    {
        CoeffFromSetting = LoadedModManager.GetMod<NoMoreRiceWorldMod>(
        ).GetSettings<NoMoreRiceWorldSettings>().VitaminsFallCoeff;
    }
}