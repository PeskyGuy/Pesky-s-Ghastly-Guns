using RimWorld;
using Verse;
using System;

namespace Pesky
{
    public class CompAbilityEffect_Exorcise : CompAbilityEffect
    {
        public new CompProperties_AbilityExorcise Props => (CompProperties_AbilityExorcise)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = parent.pawn;
            if (pawn == null) return;

            // Find the Reliquary
            ThingWithComps primary = pawn.equipment?.Primary;
            if (primary == null) return;

            CompReliquary reliquary = primary.TryGetComp<CompReliquary>();
            if (reliquary == null) return;

            int souls = reliquary.soulCount;

            // Fallback: If soulCount is 0 or 1 but hediff has more stacks, trust the hediff
            Hediff soulsHediff = pawn.health.hediffSet.GetFirstHediffOfDef(CompReliquary.ReliquarySoulsDef);
            if (soulsHediff != null && soulsHediff.Severity > souls)
            {
                souls = (int)soulsHediff.Severity;
            }

            // Perform the blast
            if (target.HasThing || target.Cell.InBounds(pawn.Map))
            {
                float finalDamage = Props.baseDamage + (souls * Props.damagePerSoul);
                
                // Perform the blast
                GenExplosion.DoExplosion(
                    center: target.Cell, 
                    map: pawn.Map, 
                    radius: 1.9f + (souls * 0.2f), 
                    damType: DamageDefOf.Psychic, 
                    instigator: pawn, 
                    damAmount: (int)finalDamage, 
                    explosionSound: SoundDefOf.PsychicPulseGlobal);

                // Visual Effects
                FleckMaker.Static(target.Cell, pawn.Map, FleckDefOf.PsycastAreaEffect, 2f);
                MoteMaker.MakeStaticMote(target.Cell, pawn.Map, ThingDefOf.Mote_PsychicLinkPulse, 1.5f);

                // Reset everything
                reliquary.ClearSouls();
                if (soulsHediff != null) pawn.health.RemoveHediff(soulsHediff);
                
                Messages.Message("PGG_ReliquaryExorcised".Translate(), pawn, MessageTypeDefOf.NeutralEvent, false);
            }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return base.CanApplyOn(target, dest) && parent.pawn.equipment?.Primary?.TryGetComp<CompReliquary>() != null;
        }
    }

    public class CompProperties_AbilityExorcise : CompProperties_AbilityEffect
    {
        public float baseDamage = 20f;
        public float damagePerSoul = 15f;

        public CompProperties_AbilityExorcise()
        {
            compClass = typeof(CompAbilityEffect_Exorcise);
        }
    }
}
