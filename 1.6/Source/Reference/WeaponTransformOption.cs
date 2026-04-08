using Verse;

namespace Pesky
{
    public class WeaponTransformOption : IExposable
    {
      public string label;
      public string description;
      public string texPath;
      public ThingDef weapon;
      public bool retainAmmoOnTransfer;
      public int ticksToTransformWeapon;
      public SoundDef sound;

      public void ExposeData()
      {
        Scribe_Defs.Look<ThingDef>(ref this.weapon, "weapon");
        Scribe_Values.Look<int>(ref this.ticksToTransformWeapon, "ticksToTransformWeapon");
      }
    }
}
