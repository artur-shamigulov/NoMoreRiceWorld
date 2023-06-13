using RimWorld;
using Verse;
using System;
using System.Diagnostics;

namespace NoMoreRiceWorld;

public enum ElementsNeeds
{
    NotDefined,
    Carbohydrates,
    Proteins,
    Vitamines,
}

public abstract class BaseFoodNeed : Need
{
    public override bool ShowOnNeedList => this.CurLevelPercentage < 0.5f;
    public override int GUIChangeArrow => this.IsFrozen ? 0 : -1;
    public static ElementsNeeds ElementsNeed = ElementsNeeds.NotDefined;
    public float FallRatePerTick => (this.pawn.needs.food.FoodFallPerTick / 10f) * currentFallCoeff * CoeffFromSetting;

    public virtual ElementsNeeds GetElementNeed => ElementsNeed;
    
    public float CoeffFromSetting = 1f;
    public abstract string FullDefName();
    public abstract string LackDefName();

    private float currentFallCoeff = 1f;
    public override void NeedInterval()
    {
        if (this.CurLevelPercentage > 0.75f)
        {
            if (Math.Abs(currentFallCoeff - 1.25f) > 0.1f)
            {
                RemoveHediff(LackDefName());
                AddHediff(FullDefName());
            }
            currentFallCoeff = 1.25f;
        }
        else if (this.CurLevelPercentage > 0.25f)
        {
            if (Math.Abs(currentFallCoeff - 1f) > 0.1f)
            {
                RemoveHediff(LackDefName());
                RemoveHediff(FullDefName());
            }
            currentFallCoeff = 1f;
        }
        else if (Math.Abs(currentFallCoeff - 0.5f) > 0.1f)
        {
            currentFallCoeff = 0.5f;
            RemoveHediff(FullDefName());
            AddHediff(LackDefName());
        }

        this.CurLevel = Math.Max(0, this.CurLevel - FallRatePerTick * 150f);
    }
    
    public BaseFoodNeed(Pawn pawn)
        : base(pawn)
    {
        this.threshPercents = new List<float>();
        this.threshPercents.Add(0.75f);
        this.threshPercents.Add(0.25f);
    }

    public void ConsumeAmount(float amount)
    {
        this.CurLevel = Math.Min(this.CurLevel + (amount / 10), this.MaxLevel);
    }

    private void RemoveHediff(string fullNameRemove)
    {
        HediffDef removeHeddifDef = DefDatabase<HediffDef>.GetNamed(fullNameRemove);
        Hediff removeHeddif = this.pawn.health.hediffSet.GetFirstHediffOfDef(removeHeddifDef);
        if (removeHeddif != null)
        {
            pawn.health.RemoveHediff(removeHeddif);
        }
    }

    private void AddHediff(string fullNameAdd)
    {
        HediffDef addHeddifDef = DefDatabase<HediffDef>.GetNamed(fullNameAdd);
        this.pawn.health.AddHediff(addHeddifDef);
    }
}
