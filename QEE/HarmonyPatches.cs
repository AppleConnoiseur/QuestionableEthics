using System;
using System.Linq;
using Verse;
using Harmony;
using System.Reflection;

namespace QEthics
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            //Harmony
            HarmonyInstance harmony = HarmonyInstance.Create("chjees.QEthics");

            //Template
            /*{
                Type type = typeof(HarmonyPatches);
                MethodInfo originalMethod = type.GetMethod("MethodName");
                HarmonyMethod patchMethod = new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_Example_Example)));
                harmony.Patch(
                    originalMethod,
                    null,
                    null);
            }*/

            {
                Type type = typeof(Pawn_EquipmentTracker);
                MethodInfo originalMethod = AccessTools.Method(type, "DropAllEquipment");
                HarmonyMethod patchMethod = new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_DropAllEquipment)));
                harmony.Patch(
                    originalMethod,
                    patchMethod,
                    null);
            }

            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static bool Patch_DropAllEquipment(Pawn ___pawn, ThingOwner<ThingWithComps> ___equipment)
        {
            Pawn pawn = ___pawn;
            if(pawn.health.Downed && pawn.health.hediffSet.hediffs.Any(hediff => hediff is Hediff_AddedPart addedPart && addedPart.comps.Any(comp => comp.props is HediffCompProperties_WeaponsPlatform)) &&
                ___equipment.InnerListForReading.Any(thing => thing.def.IsRangedWeapon))
            {
                return false;
            }

            return true;
        }
    }
}
