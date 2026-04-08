using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Pesky
{
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class Patch_GetGizmos
    {
        public static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            if (!__instance.IsColonistPlayerControlled)
                return;
            Pawn_EquipmentTracker equipment = __instance.equipment;
            CompTransformWeapon comp = equipment != null ? equipment.Primary.TryGetComp<CompTransformWeapon>() : (CompTransformWeapon)null;
            if (comp == null)
                return;
            List<Gizmo> list = __result.ToList<Gizmo>();
            list.AddRange(comp.TransformWeaponOptions());
            __result = (IEnumerable<Gizmo>)list;
        }
    }
}
