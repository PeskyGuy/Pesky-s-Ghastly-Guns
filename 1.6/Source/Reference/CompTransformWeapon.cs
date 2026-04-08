using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Pesky
{
    public class CompTransformWeapon : ThingComp    
    {
        private CompEquippable compEquippable;
        public Dictionary<ThingDef, Thing> generatedWeapons;
        private List<ThingDef> thingDefs;
        private List<Thing> things;
        public WeaponTransformOption curWeaponTransformOption;

        public CompProperties_TransformWeapon Props => this.props as CompProperties_TransformWeapon;

        private CompEquippable CompEquippable
        {
            get
            {
                if (this.compEquippable == null)
                    this.compEquippable = this.parent.GetComp<CompEquippable>();
                return this.compEquippable;
            }
        }

        public Pawn Pawn
        {
            get
            {
                return this.CompEquippable.ParentHolder is Pawn_EquipmentTracker parentHolder && parentHolder.pawn != null ? parentHolder.pawn : (Pawn)null;
            }
        }

        public IEnumerable<Gizmo> TransformWeaponOptions()
        {
            CompTransformWeapon compTransformWeapon1 = this;
            foreach (WeaponTransformOption currentOption in compTransformWeapon1.Props.weaponsToTransform)
            {
                CompTransformWeapon compTransformWeapon = compTransformWeapon1;
                Command_Action commandAction = new Command_Action();
                commandAction.defaultLabel = currentOption.label ?? (string)currentOption.weapon.LabelCap;
                commandAction.defaultDesc = currentOption.description ?? currentOption.weapon.description;
                commandAction.activateSound = SoundDefOf.Click;
                commandAction.icon = !currentOption.texPath.NullOrEmpty() ? (Texture)ContentFinder<Texture2D>.Get(currentOption.texPath) : (Texture)currentOption.weapon.uiIcon;
                commandAction.action = (Action)(() =>
                {
                    if (currentOption.ticksToTransformWeapon > 0)
                    {
                        compTransformWeapon.curWeaponTransformOption = currentOption;
                        compTransformWeapon.Pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(WeaponTransformMod.WT_TransformWeapon, (LocalTargetInfo)(Thing)compTransformWeapon.parent));
                    }
                    else
                        compTransformWeapon.TransformWeapon(currentOption);
                });
                yield return (Gizmo)commandAction;
            }
        }

        public void TransformWeapon(WeaponTransformOption weaponTransformOption)
        {
            ThingDef weapon = weaponTransformOption.weapon;
            Pawn pawn = this.Pawn;
            if (this.generatedWeapons == null)
                this.generatedWeapons = new Dictionary<ThingDef, Thing>();
            Thing thing;
            if (!this.generatedWeapons.TryGetValue(weapon, out thing))
            {
                thing = ThingMaker.MakeThing(weapon);
                this.generatedWeapons[weapon] = thing;
            }
            thing.HitPoints = this.parent.HitPoints;
            QualityCategory qc;
            if (this.parent.TryGetQuality(out qc))
            {
                CompQuality comp = thing.TryGetComp<CompQuality>();
                if (comp != null)
                    comp.SetQuality(qc, null);
            }
            this.generatedWeapons[this.parent.def] = (Thing)this.parent;
            CompTransformWeapon targetComp = thing.TryGetComp<CompTransformWeapon>();
            if (targetComp != null)
            {
                 targetComp.generatedWeapons = this.generatedWeapons;
            }

            if (pawn != null)
            {
                pawn.equipment.Remove(this.parent);
                pawn.equipment.AddEquipment(thing as ThingWithComps);
            }

            if (weaponTransformOption.sound == null)
                return;
            if (pawn != null)
            {
                weaponTransformOption.sound.PlayOneShot((SoundInfo)(Thing)pawn);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look<WeaponTransformOption>(ref this.curWeaponTransformOption, "curWeaponTransformOption");
            this.generatedWeapons?.Remove(this.parent.def);
            Scribe_Collections.Look<ThingDef, Thing>(ref this.generatedWeapons, "generatedWeapons", LookMode.Def, LookMode.Deep, ref this.thingDefs, ref this.things);
        }
    }
}
