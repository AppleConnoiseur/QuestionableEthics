using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace QEthics
{
    /// <summary>
    /// If found on a ThingDef which is a pawn it is added to the exclusion list.
    /// </summary>
    public class RaceExclusionProperties : DefModExtension
    {
        public bool excludeThisRace = true;
        public List<HediffDef> excludeTheseHediffs = new List<HediffDef>();
    }
}
