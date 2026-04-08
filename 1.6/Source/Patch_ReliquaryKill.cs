using HarmonyLib;
using RimWorld;
using System.Runtime.CompilerServices;
using System.Threading;
using Verse;

namespace Pesky
{
    [StaticConstructorOnStartup]
    public static class ReliquaryPatches
    {
        // Links projectiles to the specific Reliquary that fired them
        private static ConditionalWeakTable<Projectile, CompReliquary> projectileSources = new ConditionalWeakTable<Projectile, CompReliquary>();

        // Context tracker for the current damage application sequence
        private static ThreadLocal<CompReliquary> currentHittingComp = new ThreadLocal<CompReliquary>();

        static ReliquaryPatches()
        {
            // Harmony is initialized in WeaponTransformMod
        }

        [HarmonyPatch(typeof(Projectile), "Launch", new[] { typeof(Thing), typeof(UnityEngine.Vector3), typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(ProjectileHitFlags), typeof(bool), typeof(Thing), typeof(ThingDef) })]
        public static class Projectile_Launch_Patch
        {
            public static void Postfix(Projectile __instance, Thing launcher, Thing equipment)
            {
                if (equipment != null)
                {
                    CompReliquary comp = equipment.TryGetComp<CompReliquary>();
                    if (comp != null)
                    {
                        projectileSources.Add(__instance, comp);
                    }
                }
            }
        }

        // We patch Impact (or ImpactSomething in some versions) to set the context
        [HarmonyPatch(typeof(Projectile), "Impact")]
        public static class Projectile_Impact_Patch
        {
            public static void Prefix(Projectile __instance)
            {
                if (projectileSources.TryGetValue(__instance, out CompReliquary comp))
                {
                    currentHittingComp.Value = comp;
                }
            }

            public static void Postfix()
            {
                currentHittingComp.Value = null;
            }
        }

        [HarmonyPatch(typeof(Pawn), "Kill")]
        public static class Pawn_Kill_Patch
        {
            public static void Prefix(Pawn __instance, DamageInfo? dinfo)
            {
                if (dinfo == null) return;

                // Priority 1: Use the thread-local context (most accurate for flight-swap)
                if (currentHittingComp.Value != null)
                {
                    currentHittingComp.Value.AddSoul();
                    return;
                }

                // Priority 2: Fallback to the current equipment (for direct damage or if Impact wasn't caught)
                if (dinfo.Value.Weapon != null && dinfo.Value.Weapon.defName == "PGG_Reliquary")
                {
                    Pawn instigator = dinfo.Value.Instigator as Pawn;
                    if (instigator != null && instigator.equipment != null)
                    {
                        CompReliquary primaryComp = instigator.equipment.Primary?.TryGetComp<CompReliquary>();
                        if (primaryComp != null)
                        {
                            primaryComp.AddSoul();
                        }
                    }
                }
            }
        }
    }
}
