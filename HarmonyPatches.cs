using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Harmony;
using System.Reflection;
using Verse.AI;

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

            {
                Type type = typeof(Toils_LayDown);
                MethodInfo originalMethod = AccessTools.Method(type, "LayDown");
                HarmonyMethod patchMethod = new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_LayDown)));
                harmony.Patch(
                    originalMethod,
                    null,
                    patchMethod);
            }

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

        public static bool Patch_ShouldBeDeadFromRequiredCapacity(PawnCapacityDef __result, Pawn ___pawn)
        {
            Pawn pawn = ___pawn;
            if(pawn.health.hediffSet.HasHediff(QEHediffDefOf.QE_LifeSupport) && pawn.ValidLifeSupportNearby())
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

        public static void Patch_LayDown(ref Toil __result)
        {
            Toil toil = __result;
            if (toil == null)
                return;
            /*toil.AddPreInitAction(delegate()
            {
                Pawn actor = toil.actor;
                if(actor.ValidLifeSupportNearby())
                {
                    actor.health.AddHediff(QEHediffDefOf.QE_LifeSupport);
                }
            });*/
            toil.AddPreTickAction(delegate()
            {
                Pawn actor = toil.actor;
                if(actor != null && !actor.Dead)
                {
                    if (actor.ValidLifeSupportNearby())
                    {
                        if (!actor.health.hediffSet.HasHediff(QEHediffDefOf.QE_LifeSupport))
                        {
                            actor.health.AddHediff(QEHediffDefOf.QE_LifeSupport);
                        }
                    }
                    else
                    {
                        if (actor.health.hediffSet.GetFirstHediffOfDef(QEHediffDefOf.QE_LifeSupport, false) is Hediff hediff)
                        {
                            actor.health.RemoveHediff(hediff);
                        }
                    }
                }
            });
            /*toil.AddFinishAction(delegate()
            {
                if(toil == null)
                {
                    Log.Message("wtf, the toil is null somehow.");
                    return;
                }

                Pawn actor = toil.actor;
                if(actor != null && !actor.Dead)
                {
                    if (actor.health.hediffSet.GetFirstHediffOfDef(QEHediffDefOf.QE_LifeSupport, false) is Hediff hediff)
                    {
                        actor.health.RemoveHediff(hediff);
                    }

                    //actor.health.CheckForStateChange(null, null);
                }
            });*/
        }
    }
}
