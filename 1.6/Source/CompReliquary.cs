using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace Pesky
{
    public class CompReliquary : ThingComp
    {
        public int soulCount = 0;
        public const int MaxSouls = 20;

        public CompProperties_Reliquary Props => (CompProperties_Reliquary)props;

        // The hediff we apply to the wielder
        public static HediffDef ReliquarySoulsDef => HediffDef.Named("PGG_ReliquarySouls");

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref soulCount, "soulCount", 0);
        }

        public void AddSoul()
        {
            if (soulCount < MaxSouls)
            {
                soulCount++;
                UpdateWielderHediff();
                Messages.Message("PGG_ReliquarySoulCaptured".Translate(), parent, MessageTypeDefOf.NeutralEvent, false);
            }
        }

        public void ClearSouls()
        {
            soulCount = 0;
            UpdateWielderHediff();
        }

        public void UpdateWielderHediff()
        {
            Pawn wielder = GetWielder();
            if (wielder == null) return;

            Hediff soulsHediff = wielder.health.hediffSet.GetFirstHediffOfDef(ReliquarySoulsDef);

            if (soulCount > 0)
            {
                if (soulsHediff == null)
                {
                    soulsHediff = HediffMaker.MakeHediff(ReliquarySoulsDef, wielder);
                    wielder.health.AddHediff(soulsHediff);
                }
                // We use severity to track stacks (1.0 = 1 soul, etc. for stage transitions)
                soulsHediff.Severity = (float)soulCount;
            }
            else if (soulsHediff != null)
            {
                wielder.health.RemoveHediff(soulsHediff);
            }
        }

        public Pawn GetWielder()
        {
            if (parent.ParentHolder is Pawn_EquipmentTracker tracker)
            {
                return tracker.pawn;
            }
            return null;
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            UpdateWielderHediff();
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            // Remove hediff when unequipped
            Hediff soulsHediff = pawn.health.hediffSet.GetFirstHediffOfDef(ReliquarySoulsDef);
            if (soulsHediff != null)
            {
                pawn.health.RemoveHediff(soulsHediff);
            }
        }
    }

    public class CompProperties_Reliquary : CompProperties
    {
        public CompProperties_Reliquary()
        {
            this.compClass = typeof(CompReliquary);
        }
    }
}
