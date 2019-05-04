using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace QEthics
{
    /// <summary>
    /// Stores general compatibility information for the Genome sequencer and Brain scanner.
    /// </summary>
    public static class GeneralCompatibility
    {
        public static List<ThingDef> excludedRaces = new List<ThingDef>();
        public static List<HediffDef> excludedHediffs = new List<HediffDef>();
    }
}
