using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace QEthics
{
    /// <summary>
    /// Points to the ThingDef for the GenomeSequencer.
    /// </summary>
    public class RecipeOutcomeProperties : DefModExtension
    {
        public ThingDef outputThingDef;
        public HediffDef outcomeHediff;
        public float outcomeHediffPotency = 1f;
    }
}
