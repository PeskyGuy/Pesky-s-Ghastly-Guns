using HarmonyLib;
using Verse;

namespace Pesky
{
    public class WeaponTransformMod : Mod
    {
      public static JobDef WT_TransformWeapon;

      public WeaponTransformMod(ModContentPack content)
        : base(content)
      {
        new Harmony("WeaponTransform.Mod").PatchAll();
      }
    }
}
