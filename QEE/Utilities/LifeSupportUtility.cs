using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace QEthics
{
    public static class LifeSupportUtility
    {
        public static bool ValidLifeSupportNearby(this Pawn pawn)
        {
            return pawn.CurrentBed() is Building_Bed bed &&
                GenAdj.CellsAdjacent8Way(bed).Any(cell => cell.GetThingList(bed.Map).Any(cellThing => cellThing.TryGetComp<LifeSupportComp>() is LifeSupportComp lifeSupport && lifeSupport.Active));
        }
    }
}
