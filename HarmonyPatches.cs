using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
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
                Type type = typeof(Pawn_HealthTracker);
                MethodInfo originalMethod = AccessTools.Method(type, "ShouldBeDeadFromRequiredCapacity");
                HarmonyMethod patchMethod = new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_ShouldBeDeadFromRequiredCapacity)));
                harmony.Patch(
                    originalMethod,
                    patchMethod,
                    null);
            }

            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static bool Patch_ShouldBeDeadFromRequiredCapacity(PawnCapacityDef __result, Pawn ___pawn)
        {
            Pawn pawn = ___pawn;
            if(pawn.CurrentBed() is Building_Bed bed && 
                GenAdj.CellsAdjacent8Way(bed).Any(cell => cell.GetThingList(bed.Map).Any(cellThing => cellThing.TryGetComp<LifeSupportComp>() is LifeSupportComp lifeSupport && lifeSupport.Active)))
            {
                //Check if consciousness is there. If it is then its okay.
                PawnCapacityDef pawnCapacityDef = PawnCapacityDefOf.Consciousness;
                bool flag = (!pawn.RaceProps.IsFlesh) ? pawnCapacityDef.lethalMechanoids : pawnCapacityDef.lethalFlesh;
                if (flag && pawn.health.capacities.CapableOf(pawnCapacityDef))
                {
                    __result = pawnCapacityDef;
                    return false;
                }

                __result = null;
                return false;
            }

            return true;
        }
    }
}
