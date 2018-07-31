using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;
using Verse;
using RimWorld;

namespace QEthics
{
    public class JobDriver_DepositIntoGrower : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!pawn.CanReserve(TargetThingA))
            {
                return false;
            }

            if (!pawn.CanReserve(TargetThingB))
            {
                return false;
            }

            return pawn.Reserve(TargetThingA, job, errorOnFailed: errorOnFailed) && pawn.Reserve(TargetThingB, job, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Conditions and reserve
            this.FailOn(delegate ()
            {
                return TargetThingA is Building_GrowerBase vat && vat.status != CrafterStatus.Filling;
            });
            this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
            this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.B);

            //Go and get the thing to carry.
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, subtractNumTakenFromJobCount: true);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            yield return Toils_General.WaitWith(TargetIndex.A, 100, true);
            yield return new Toil()
            {
                initAction = delegate ()
                {
                    Building_GrowerBase grower = TargetThingA as Building_GrowerBase;
                    if (grower != null)
                    {
                        Pawn actor = GetActor();
                        grower.FillThing(TargetThingB);
                    }
                }
            };
        }
    }
}
