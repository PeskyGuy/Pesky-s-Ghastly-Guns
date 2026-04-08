using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Pesky
{
    public class HediffConfig : IExposable
    {
        public HediffDef hediffToApply;
        public BodyPartDef bodyPartToAffect = null;
        public float hediffSeverity = 1.0f;
        public bool removeWhenUnequipped = true;
        public bool applyOnUnequip = false;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref hediffToApply, "hediffToApply");
            Scribe_Defs.Look(ref bodyPartToAffect, "bodyPartToAffect");
            Scribe_Values.Look(ref hediffSeverity, "hediffSeverity", 1.0f);
            Scribe_Values.Look(ref removeWhenUnequipped, "removeWhenUnequipped", true);
            Scribe_Values.Look(ref applyOnUnequip, "applyOnUnequip", false);
        }
    }

    public class CompProperties_WeaponHediff : CompProperties
    {
        public List<HediffConfig> hediffs = new List<HediffConfig>();

        public CompProperties_WeaponHediff()
        {
            compClass = typeof(CompWeaponHediff);
        }
    }

    public class CompWeaponHediff : ThingComp
    {
        public CompProperties_WeaponHediff Props => (CompProperties_WeaponHediff)props;

        private Pawn equipper = null;
        private List<Hediff> appliedHediffs = new List<Hediff>();
        private List<Hediff> unequipHediffs = new List<Hediff>();

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            equipper = pawn;
            RemoveUnequipHediffs();
            ApplyEquipHediffs();
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            RemoveEquipHediffs();
            ApplyUnequipHediffs();
            equipper = null;
        }

        private void ApplyEquipHediffs()
        {
            if (equipper == null || equipper.Dead) return;

            foreach (var hediffConfig in Props.hediffs)
            {
                if (hediffConfig.applyOnUnequip || hediffConfig.hediffToApply == null) continue;

                var hediff = ApplyHediff(hediffConfig);
                if (hediff != null)
                {
                    appliedHediffs.Add(hediff);
                }
            }
        }

        private void ApplyUnequipHediffs()
        {
            if (equipper == null || equipper.Dead) return;

            foreach (var hediffConfig in Props.hediffs)
            {
                if (!hediffConfig.applyOnUnequip || hediffConfig.hediffToApply == null) continue;

                var hediff = ApplyHediff(hediffConfig);
                if (hediff != null)
                {
                    unequipHediffs.Add(hediff);
                }
            }
        }

        private Hediff ApplyHediff(HediffConfig config)
        {
            BodyPartRecord bodyPartRecord = null;
            if (config.bodyPartToAffect != null)
            {
                bodyPartRecord = equipper.RaceProps.body.GetPartsWithDef(config.bodyPartToAffect).FirstOrDefault();
                if (bodyPartRecord == null)
                {
                    Log.Warning($"[CompWeaponHediff] Could not find body part {config.bodyPartToAffect.defName} on {equipper.Name} for hediff {config.hediffToApply.defName}. Applying to whole body.");
                }
            }

            var hediff = HediffMaker.MakeHediff(config.hediffToApply, equipper, bodyPartRecord);
            hediff.Severity = config.hediffSeverity;
            equipper.health.AddHediff(hediff);
            return hediff;
        }

        private void RemoveEquipHediffs()
        {
            if (equipper == null || equipper.health == null) return;

            for (int i = appliedHediffs.Count - 1; i >= 0; i--)
            {
                var hediff = appliedHediffs[i];
                var config = Props.hediffs.FirstOrDefault(h => !h.applyOnUnequip);

                if (config != null && config.removeWhenUnequipped)
                {
                    if (equipper.health.hediffSet.hediffs.Contains(hediff))
                    {
                        equipper.health.RemoveHediff(hediff);
                    }
                }
                appliedHediffs.RemoveAt(i);
            }
        }

        private void RemoveUnequipHediffs()
        {
            if (equipper == null || equipper.health == null) return;

            for (int i = unequipHediffs.Count - 1; i >= 0; i--)
            {
                var hediff = unequipHediffs[i];
                if (equipper.health.hediffSet.hediffs.Contains(hediff))
                {
                    equipper.health.RemoveHediff(hediff);
                }
                unequipHediffs.RemoveAt(i);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref equipper, "equipper");
            Scribe_Collections.Look(ref appliedHediffs, "appliedHediffs", LookMode.Reference);
            Scribe_Collections.Look(ref unequipHediffs, "unequipHediffs", LookMode.Reference);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                appliedHediffs = appliedHediffs ?? new List<Hediff>();
                unequipHediffs = unequipHediffs ?? new List<Hediff>();

                CleanupHediffList(appliedHediffs);
                CleanupHediffList(unequipHediffs);
            }
        }

        private void CleanupHediffList(List<Hediff> hediffs)
        {
            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                if (hediffs[i] == null || 
                    equipper == null || 
                    equipper.Dead || 
                    !equipper.health.hediffSet.hediffs.Contains(hediffs[i]))
                {
                    hediffs.RemoveAt(i);
                }
            }
        }
    }
}