using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;
using Verse;
using RimWorld;

namespace QEthics
{
    public class JobDriver_UseBrainTemplate : JobDriver
    {
        public static float workRequired = 2000;

        public float ticksWork;
        public bool workStarted;

        public Pawn Patient
        {
            get
            {
                return TargetThingA as Pawn;
            }
        }

        public BrainScanTemplate BrainTemplate
        {
            get
            {
                return TargetThingB as BrainScanTemplate;
            }
        }

        public Building_Bed Bed
        {
            get
            {
                return job.targetC.Thing as Building_Bed;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!pawn.CanReserve(TargetThingA))
            {
                return false;
            }

            else  if (!pawn.CanReserve(TargetThingB))
            {
                return false;
            }

            else if (!pawn.CanReserve(job.targetC.Thing) && Patient.CurrentBed() != Bed)
            {
                return false;
            }

            return true;
            //return pawn.Reserve(TargetThingA, job, errorOnFailed: errorOnFailed) && pawn.Reserve(TargetThingB, job, errorOnFailed: errorOnFailed) && pawn.Reserve(job.targetC.Thing, job, errorOnFailed: errorOnFailed);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref ticksWork, "ticksWork"); //workStarted
            Scribe_Values.Look(ref workStarted, "workStarted");
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.AddFinishAction(delegate()
            {
                if(Patient.CurrentBed() == Bed && Patient.CurJobDef == JobDefOf.LayDown)
                {
                    Patient.jobs.EndCurrentJob(JobCondition.Succeeded);
                }
            });
            //A - Sleeper   B - Brain template   C - Bed

            this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
            this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
            this.FailOnDestroyedNullOrForbidden(TargetIndex.C);
            
            yield return Toils_Reserve.Reserve(TargetIndex.B);
            
            if(Patient.CurJobDef != JobDefOf.LayDown)
            {
                QEEMod.TryLog("Patient not in bed, carrying them to bed");
                //get the patient and carry them to the bed
                yield return Toils_Reserve.Reserve(TargetIndex.A);
                yield return Toils_Reserve.Reserve(TargetIndex.C);
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
                yield return Toils_Haul.StartCarryThing(TargetIndex.A);
                yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.InteractionCell);
                yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, null, false);
                yield return Toils_Reserve.Release(TargetIndex.C);
            }

            //Surgeon gets the brain template, carries it, then travel to bed
            QEEMod.TryLog("Carrying brain template to bed");
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.ClosestTouch);

            Toil applyBrainTemplateToil = new Toil()
            {
                defaultCompleteMode = ToilCompleteMode.Never,
                initAction = delegate ()
                {
                    if (Patient.CurJobDef != JobDefOf.LayDown)
                    {
                        Patient.pather.StopDead();
                        Patient.jobs.StartJob(new Job(JobDefOf.LayDown, TargetC)
                        {
                            forceSleep = true
                        });
                    }

                    Toils_Reserve.Reserve(TargetIndex.A);

                },
                tickAction = delegate()
                {
                    ticksWork -= 1f * pawn.GetStatValue(StatDefOf.ResearchSpeed);

                    if (ticksWork <= 0)
                    {
                        BrainManipUtility.ApplyBrainScanTemplateOnPawn(Patient, BrainTemplate);
                        Patient.jobs.EndCurrentJob(JobCondition.Succeeded);

                        //Give headache
                        Patient.health.AddHediff(QEHediffDefOf.QE_Headache, Patient.health.hediffSet.GetBrain());
                    }
                }
            }.WithProgressBar(TargetIndex.A, () => (workRequired - ticksWork) / workRequired).WithEffect(EffecterDefOf.Research, TargetIndex.A);
            applyBrainTemplateToil.AddPreInitAction(delegate()
            {
                ticksWork = workRequired;
                workStarted = true;
                //Log.Message("preInitAction: ticksWork = " + ticksWork);
            });
            applyBrainTemplateToil.AddEndCondition(delegate()
            {
                if(workStarted && ticksWork <= 0)
                {
                    //Log.Message("Succeeded: ticksWork = " + ticksWork);
                    return JobCondition.Succeeded;
                }

                //Log.Message("Ongoing: ticksWork = " + ticksWork);
                return JobCondition.Ongoing;
            });
            applyBrainTemplateToil.AddFailCondition(() => workStarted && Patient.CurJobDef != JobDefOf.LayDown);

            yield return applyBrainTemplateToil;

        }
    }
}
