using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace QEthics
{
    [StaticConstructorOnStartup]
    public static class PawnUtility
    {
        static PawnUtility()
        {
            Reset();
        }

        public static Building_Bed FindSuitableSurgeryBed(this Pawn sleeper, Pawn traveler)
        {
            bool sleeperWillBePrisoner = sleeper.IsPrisoner;

            if (sleeper.InBed()/* && sleeper.CurrentBed().Medical*/)
            {
                Building_Bed bedThing = sleeper.CurrentBed();
                if (RestUtility.IsValidBedFor(bedThing, sleeper, traveler, sleeperWillBePrisoner, false))
                {
                    return sleeper.CurrentBed();
                }
            }

            for (int i = 0; i < bedDefsBestToWorst_Medical.Count; i++)
            {
                ThingDef thingDef = bedDefsBestToWorst_Medical[i];
                if (RestUtility.CanUseBedEver(sleeper, thingDef))
                {
                    for (int j = 0; j < 2; j++)
                    {
                        Danger maxDanger = (j != 0) ? Danger.Deadly : Danger.None;
                        Building_Bed building_Bed = (Building_Bed)GenClosest.ClosestThingReachable(sleeper.Position, sleeper.Map, ThingRequest.ForDef(thingDef), PathEndMode.OnCell, TraverseParms.For(traveler, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, delegate (Thing b)
                        {
                            //Log.Message("Bed=" + b.ThingID + "; def=" + thingDef.defName);
                            Building_Bed bed = b as Building_Bed;

                            bool result;
                            if (/*((Building_Bed)b).Medical &&*/ b.Position.GetDangerFor(sleeper, sleeper.Map) <= maxDanger)
                            {
                                /*result = 
                                RestUtility.CanUseBedEver(sleeper, thingDef) && 
                                traveler.CanReserveAndReach(bed, PathEndMode.OnCell, Danger.Deadly) &&
                                bed.AnyUnoccupiedSleepingSlot && 
                                !bed.IsForbidden(sleeper) && 
                                !bed.IsBurning() && 
                                (sleeperWillBePrisoner ? bed.ForPrisoners : bed.Faction == null || bed.Faction == sleeper.Faction);*/
                                result = RestUtility.IsValidBedFor(b, sleeper, traveler, sleeperWillBePrisoner, false);
                            }
                            else
                            {
                                result = false;
                            }
                            return result;
                        }, null, 0, -1, false, RegionType.Set_Passable, false);

                        if (building_Bed != null)
                        {
                            return building_Bed;
                        }
                    }
                }
            }

            return null;
        }

        public static void Reset()
        {
            bedDefsBestToWorst_Medical = (from d in DefDatabase<ThingDef>.AllDefs
                                          where d.IsBed
                                          orderby d.building.bed_maxBodySize, d.GetStatValueAbstract(StatDefOf.MedicalTendQualityOffset, null) descending, d.GetStatValueAbstract(StatDefOf.BedRestEffectiveness, null) descending
                                          select d).ToList<ThingDef>();
        }

        private static List<ThingDef> bedDefsBestToWorst_Medical;
    }
}
