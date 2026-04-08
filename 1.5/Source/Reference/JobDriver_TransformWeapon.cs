using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Pesky
{
    public class JobDriver_TransformWeapon : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            CompTransformWeapon comp = this.TargetA.Thing.TryGetComp<CompTransformWeapon>();
            yield return Toils_General.Wait(comp.curWeaponTransformOption.ticksToTransformWeapon).WithProgressBarToilDelay(TargetIndex.A);
            yield return Toils_General.Do((Action)(() => comp.TransformWeapon(comp.curWeaponTransformOption)));
        }
    }
}
