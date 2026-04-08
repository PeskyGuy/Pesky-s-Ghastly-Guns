using HarmonyLib;
using RimWorld;
using Verse;

namespace Pesky
{
    [HarmonyPatch(typeof(DefGenerator), "GenerateImpliedDefs_PreResolve")]
    public class Patch_GenerateImpliedDefs_PreResolve
    {
        public static void Postfix()
        {
            JobDef jobDef = new JobDef();
            jobDef.defName = "WT_TransformWeapon";
            jobDef.driverClass = typeof(JobDriver_TransformWeapon);
            WeaponTransformMod.WT_TransformWeapon = jobDef;
            DefGenerator.AddImpliedDef<JobDef>(WeaponTransformMod.WT_TransformWeapon);
        }
    }
}
