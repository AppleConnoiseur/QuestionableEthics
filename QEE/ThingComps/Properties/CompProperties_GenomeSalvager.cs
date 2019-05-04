using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace QEthics
{
    public class CompProperties_GenomeSalvager : CompProperties
    {
        public JobDef salvagingJob;

        public CompProperties_GenomeSalvager()
        {
            compClass = typeof(GenomeSalvagerComp);
        }
    }
}
