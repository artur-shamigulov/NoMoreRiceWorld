using Verse;
using RimWorld;

namespace NoMoreRiceWorld;

public enum FoodVariatyCategory : byte
{
    Empty,
    VeryLow,
    Low,
    Satisfied,
    High,
    Extreme,
} 

public class FoodVariatyNeed : Need
{
    public override bool ShowOnNeedList => alwaysShowVarietyNeed || Prefs.DevMode || this.CurLevelPercentage < 0.3f;
    public override int GUIChangeArrow => this.IsFrozen ? 0 : -1;
    public new void SetInitialLevel() => this.CurLevelPercentage = 1f;
    public FoodVariatyToleranceSet Tolerance;
    public float FallCoeffFromSetting = 1f;

    private bool alwaysShowVarietyNeed = false;

    public FoodVariatyCategory CurCategory
    {
        get
        {
            if ((double) this.CurLevel < 0.001)
                return FoodVariatyCategory.Empty;
            if ((double) this.CurLevel < 0.15f)
                return FoodVariatyCategory.VeryLow;
            if ((double) this.CurLevel < 0.30f)
                return FoodVariatyCategory.Low;
            if ((double) this.CurLevel < 0.70f)
                return FoodVariatyCategory.Satisfied;
            return (double) this.CurLevel < 0.85f ? FoodVariatyCategory.High : FoodVariatyCategory.Extreme;
        }
    }

    public override string GetTipString()
    {
        TaggedString taggedString = (TaggedString)base.GetTipString();
        string str = Tolerance.TolerancesString();
        if (!string.IsNullOrEmpty(str))
            taggedString += "\n\n" + str;
        return taggedString.Resolve();
    }

    private float FallPerInterval
    {
        get
        {
            switch (this.CurCategory)
            {
                case FoodVariatyCategory.Empty:
                    return 0.0015f;
                case FoodVariatyCategory.VeryLow:
                    return 0.0006f;
                case FoodVariatyCategory.Low:
                    return 0.00105f;
                case FoodVariatyCategory.Satisfied:
                    return 0.0015f;
                case FoodVariatyCategory.High:
                    return 0.0015f;
                case FoodVariatyCategory.Extreme:
                    return 0.0015f;
                default:
                    throw new InvalidOperationException();
            }
        }
    }

    private bool IsGourmand = false;

    private float FallPerIntervalWithTrait
    {
        get {
            return IsGourmand ? FallPerInterval * 2 : FallPerInterval;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Tolerance.ExposeData();
    }

    public override void NeedInterval()
    {
        this.CurLevel -= FallPerIntervalWithTrait * FallCoeffFromSetting;
        Tolerance.NeedInterval();
    }

    public void ConsumeFood(Thing food, float amount)
    {
        if (food.def.IsNutritionGivingIngestible)
        {
            float coeff = Tolerance.ConsumeFood(food);
            CurLevel = Math.Min(MaxLevel, coeff * amount + CurLevel);
        }
    }

    public FoodVariatyNeed(Pawn pawn)
        : base(pawn)
    {
        this.threshPercents = new List<float>();
        this.threshPercents.Add(0.85f);
        this.threshPercents.Add(0.7f);
        this.threshPercents.Add(0.3f);
        this.threshPercents.Add(0.15f);
        
        SetInitialLevel();

        Tolerance = new FoodVariatyToleranceSet();
        
        var settings = LoadedModManager.GetMod<NoMoreRiceWorldMod>(
        ).GetSettings<NoMoreRiceWorldSettings>();
        FallCoeffFromSetting = settings.FoodVarietyFallCoeff;
        alwaysShowVarietyNeed = settings.ShowVarietyNeeds;
        
        List<Trait> allTraits = pawn.story?.traits?.allTraits;
        if (allTraits != null)
        {
            if (!allTraits.NullOrEmpty<Trait>())
            {
                foreach (Trait trait in allTraits)
                {
                    if (trait.def.defName == "Gourmand")
                    {
                        IsGourmand = true;
                    }
                }
            }
        }
    }
}