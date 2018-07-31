using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace QEthics
{
    public class CompProperties_CustomName : CompProperties
    {
        public CompProperties_CustomName()
        {
            compClass = typeof(CustomNameComp);
        }
    }
}
