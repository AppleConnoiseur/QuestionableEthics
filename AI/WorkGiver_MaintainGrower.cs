using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace QEthics
{
    public class WorkGiver_MaintainGrower : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(def.fixedBillGiverDefs.FirstOrDefault());

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_GrowerBase grower = t as Building_GrowerBase;
            if (grower == null)
            {
                return false;
            }

            IMaintainableGrower maintanable = grower as IMaintainableGrower;
            if (maintanable == null)
            {
                return false;
            }

            if (t.IsForbidden(pawn))
            {
                return false;
            }

            if (!pawn.CanReserve(t))
            {
                return false;
            }

            if (grower.status != CrafterStatus.Crafting)
            {
                return false;
            }

            if((maintanable.ScientistMaintence > 0.35f) && def.workType == QEWorkTypeDefOf.Research)
            {
                if (maintanable.ScientistMaintence > 0.90f)
                    return false;
                return forced;
            }

            if ((maintanable.DoctorMaintence > 0.35f) && def.workType == WorkTypeDefOf.Doctor)
            {
                if (maintanable.DoctorMaintence > 0.90f)
                    return false;
                return forced;
            }

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            //Building_GrowerBase grower = t as Building_GrowerBase;
            Job job = null;
            if(def.workType == QEWorkTypeDefOf.Research)
            {
                job = new Job(QEJobDefOf.QE_MaintainGrowerJob_Intellectual, t);
            }
            else if(def.workType == WorkTypeDefOf.Doctor)
            {
                job = new Job(QEJobDefOf.QE_MaintainGrowerJob_Medicine, t);
            }

            return job;
        }
    }
}
