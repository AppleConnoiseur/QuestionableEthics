using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace QEthics
{
    public class JobDriver_SalvageGenome : JobDriver
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
            this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
            this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.B);

            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, subtractNumTakenFromJobCount: true);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return Toils_General.WaitWith(TargetIndex.A, 600, true);
            yield return new Toil()
            {
                initAction = delegate()
                {
                    //Destroy tool.
                    TargetThingB.Destroy();

                    //Roughly 60% chance to succeed. Modified by the medical surgery chance.
                    if (!Rand.Chance(0.6f * pawn.GetStatValue(StatDefOf.MedicalSurgerySuccessChance, true)))
                    {
                        return;
                    }

                    RecipeOutcomeProperties props = job.def.GetModExtension<RecipeOutcomeProperties>() ?? null;
                    if (props != null)
                    {
                        Corpse corpse = TargetThingA as Corpse;
                        Thing genomeSequence = GenomeUtility.MakeGenomeSequence(corpse.InnerPawn, props.outputThingDef);
                        GenPlace.TryPlaceThing(genomeSequence, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                    }
                }
            };
        }
    }
}
