using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace QEthics
{
    /// <summary>
    /// Tags this Thing as being valid life support.
    /// </summary>
    public class LifeSupportComp : ThingComp
    {
        public bool Active
        {
            get
            {
                if(parent.TryGetComp<CompPowerTrader>() is CompPowerTrader power)
                {
                    return power.PowerOn;
                }

                return true;
            }
        }

        public override void ReceiveCompSignal(string signal)
        {
            if(signal == "PowerTurnedOn" || signal == "PowerTurnedOff")
            {
                //Check for state change in surrounding pawns in beds.
                Map map = parent.Map;
                foreach (IntVec3 cell in parent.CellsAdjacent8WayAndInside())
                {
                    List<Thing> things = cell.GetThingList(map);
                    foreach(Thing thing in things)
                    {
                        if(thing is Building_Bed bed)
                        {
                            for(int i = 0; i < bed.SleepingSlotsCount; i++)
                            {
                                IntVec3 sleepingSpot = bed.GetSleepingSlotPos(i);
                                Pawn pawn = sleepingSpot.GetFirstPawn(map);
                                if(pawn != null)
                                {
                                    pawn.health.CheckForStateChange(null, null);
                                    //Log.Message("Pawn found in bed: " + pawn.LabelCap);
                                }
                            }
                        }
                    }
                }

                //Check for state change in surrounding pawns.
                /*foreach (IntVec3 cell in GenRadial.RadialCellsAround(parent.Position, 3.5f, false))
                {
                    if(cell.GetFirstPawn(parent.Map) is Pawn pawn)
                    {
                        pawn.health.CheckForStateChange(null, null);
                        Log.Message("Pawn found: " + pawn.LabelCap);
                    }
                }*/
            }
        }
    }
}
