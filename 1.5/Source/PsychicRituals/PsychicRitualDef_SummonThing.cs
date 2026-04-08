using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Pesky
{
    public class PsychicRitualDef_SummonThing : PsychicRitualDef_InvocationCircle
    {
        public ThingDef thingToSummon;
        public SimpleCurve countFromQualityCurve;

        public SimpleCurve backlashChanceFromQualityCurve;
        public HediffDef backlashHediff;
        public FloatRange backlashSeverityRange = new FloatRange(0.1f, 0.5f);

        public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph graph)
        {
            List<PsychicRitualToil> toils = base.CreateToils(psychicRitual, graph);

            toils.Add(new PsychicRitualToil_SummonThing(this.InvokerRole, this));

            if (this.backlashHediff != null && this.backlashChanceFromQualityCurve != null)
            {
                 toils.Add(new PsychicRitualToil_ApplySummoningBacklash(this.InvokerRole, this.backlashHediff, this.backlashChanceFromQualityCurve, this.backlashSeverityRange));
            }
            else
            {
                Log.Warning($"PsychicRitualDef {this.defName} is missing backlash configuration (backlashHediff or backlashChanceFromQualityCurve). Backlash toil not added.");
            }


            return toils;
        }

        public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
        {
            if (this.outcomeDescription.NullOrEmpty())
            {
                 Log.ErrorOnce($"PsychicRitualDef {this.defName} is missing outcomeDescription.", this.GetHashCode() ^ 54321);
                 return "Error: Missing outcome description.";
            }

            float minQuality = qualityRange.min;
            int minCount = (int)this.countFromQualityCurve.Evaluate(minQuality);
            float backlashChance = (this.backlashChanceFromQualityCurve != null) ? this.backlashChanceFromQualityCurve.Evaluate(minQuality) : 0f;


            return this.outcomeDescription.Formatted(
                minCount.ToString(),
                backlashChance.ToStringPercent() 
                );
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors())
            {
                yield return error;
            }
            if (this.thingToSummon == null) yield return $"{nameof(thingToSummon)} is not defined.";
            if (this.countFromQualityCurve == null) yield return $"{nameof(countFromQualityCurve)} is not defined.";
            if (this.backlashChanceFromQualityCurve == null) yield return $"{nameof(backlashChanceFromQualityCurve)} is not defined.";
            if (this.backlashHediff == null) yield return $"{nameof(backlashHediff)} is not defined.";
             if (this.backlashHediff != null && this.backlashChanceFromQualityCurve != null && !this.outcomeDescription.NullOrEmpty() && (!this.outcomeDescription.Contains("{0}") || !this.outcomeDescription.Contains("{1}")))
             {
                yield return $"{nameof(outcomeDescription)} should contain placeholders for {{0}} (count) and {{1}} (backlash chance). Current: '{this.outcomeDescription}'";
             }
             else if (this.outcomeDescription.NullOrEmpty())
             {
                 yield return $"{nameof(outcomeDescription)} is missing or empty.";
             }
             else if ((this.backlashHediff == null || this.backlashChanceFromQualityCurve == null) && !this.outcomeDescription.Contains("{0}"))
             {
                 yield return $"{nameof(outcomeDescription)} should contain placeholder for {{0}} (count). Current: '{this.outcomeDescription}'";
             }
        }
    }
} 