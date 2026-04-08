using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Pesky
{
    public class PsychicRitualToil_RemoveCurses : PsychicRitualToil
    {
        private PsychicRitualRoleDef targetRole;

        public PsychicRitualToil_RemoveCurses(PsychicRitualRoleDef targetRole)
        {
            this.targetRole = targetRole;
        }

        public override void Start(PsychicRitual ritual, PsychicRitualGraph graph)
        {
            base.Start(ritual, graph);

            Pawn target = ritual.assignments.FirstAssignedPawn(targetRole);
            if (target == null)
            {
                Log.Error($"[RemoveCursesToil] No target pawn found for role {targetRole?.defName ?? "null"}.");
                return;
            }

            List<Hediff> hediffsToRemove = new List<Hediff>();
            foreach (Hediff hediff in target.health.hediffSet.hediffs)
            {
                if (hediff.def.tags != null && hediff.def.tags.Contains("PGG_Curse"))
                {
                    hediffsToRemove.Add(hediff);
                }
            }

            int count = hediffsToRemove.Count;
            if (count > 0)
            {
                foreach (Hediff h in hediffsToRemove)
                {
                    target.health.RemoveHediff(h);
                }
                Messages.Message("PGG_CursesRemoved".Translate(target.LabelShortCap, count), target, MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Messages.Message("PGG_NoCursesFound".Translate(target.LabelShortCap), target, MessageTypeDefOf.NeutralEvent);
            }
        }

        public PsychicRitualToil_RemoveCurses() { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref targetRole, "targetRole");
        }
    }
}
