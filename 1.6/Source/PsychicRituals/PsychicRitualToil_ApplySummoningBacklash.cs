using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Pesky
{
    public class PsychicRitualToil_ApplySummoningBacklash : PsychicRitualToil
    {
        private PsychicRitualRoleDef roleDef;
        private HediffDef hediffDef;
        private SimpleCurve chanceFromQualityCurve;
        private FloatRange severityRange;

        public PsychicRitualToil_ApplySummoningBacklash(PsychicRitualRoleDef roleDef, HediffDef hediffDef, SimpleCurve chanceFromQualityCurve, FloatRange severityRange)
        {
            this.roleDef = roleDef;
            this.hediffDef = hediffDef;
            this.chanceFromQualityCurve = chanceFromQualityCurve;
            this.severityRange = severityRange;
        }

        public override void Start(PsychicRitual ritual, PsychicRitualGraph graph)
        {
            base.Start(ritual, graph);

            Pawn invoker = ritual.assignments.FirstAssignedPawn(roleDef);
            if (invoker == null || hediffDef == null || chanceFromQualityCurve == null)
            {
                 Log.Error($"[ApplySummoningBacklash] Toil started with missing data. Ritual: {ritual.def?.LabelCap ?? "Unknown"}");
                return;
            }

            List<QualityFactor> qualityFactorsRuntime = new List<QualityFactor>();
            ritual.def.CalculateMaxPower(ritual.assignments, qualityFactorsRuntime, out float quality);

            float chance = chanceFromQualityCurve.Evaluate(quality);

            if (Rand.Chance(chance))
            {
                Hediff hediff = HediffMaker.MakeHediff(hediffDef, invoker);
                hediff.Severity = severityRange.RandomInRange;
                invoker.health.AddHediff(hediff);

                Messages.Message("PGG_SummoningBacklashSuffered".Translate(invoker.Named("PAWN"), hediffDef.label), invoker, MessageTypeDefOf.NegativeEvent, historical: false);
            }
        }

        public PsychicRitualToil_ApplySummoningBacklash() { }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref roleDef, "roleDef");
            Scribe_Defs.Look(ref hediffDef, "hediffDef");
            Scribe_Deep.Look(ref chanceFromQualityCurve, "chanceFromQualityCurve");
            Scribe_Values.Look(ref severityRange, "severityRange");
        }
    }
} 