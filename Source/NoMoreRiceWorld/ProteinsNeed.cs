using Verse;

namespace NoMoreRiceWorld;

public class ProteinsNeed: BaseFoodNeed
{
    public new static ElementsNeeds ElementsNeed = ElementsNeeds.Proteins;
    public override ElementsNeeds GetElementNeed => ElementsNeed;
    
    public override string FullDefName()
    {
        return "ProteinsFull";
    }

    public override string LackDefName()
    {
        return "ProteinsLack";
    }
    
    public ProteinsNeed(Pawn pawn)
        : base(pawn)
    {
    }
}