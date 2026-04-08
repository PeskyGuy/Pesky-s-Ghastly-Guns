using System.Collections.Generic;
using Verse;

namespace Pesky
{
    public class CompProperties_TransformWeapon : CompProperties
    {
      public List<WeaponTransformOption> weaponsToTransform;

      public CompProperties_TransformWeapon() => this.compClass = typeof (CompTransformWeapon);
    }
}
