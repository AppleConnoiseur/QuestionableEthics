using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace QEthics
{
    /// <summary>
    /// Simply removes itself if the pawn no longer is in a bed.
    /// </summary>
    public class Hediff_LifeSupport : HediffWithComps
    {
        public override bool ShouldRemove => pawn.CurrentBed() == null;
    }
}
