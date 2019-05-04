using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.Sound;
using Verse.AI;

namespace QEthics
{
    public class GenomeSalvagerComp : ThingComp
    {
        public CompProperties_GenomeSalvager SalvagerProps
        {
            get
            {
                return props as CompProperties_GenomeSalvager;
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            //Start targeter
            if(selPawn.skills.GetSkill(SkillDefOf.Medicine).TotallyDisabled)
            {
                yield break;
            }

            float chance = 0.6f * selPawn.GetStatValue(StatDefOf.MedicalSurgerySuccessChance, true);
            yield return new FloatMenuOption("QE_GenomeSequencerSalvage".Translate(chance.ToStringPercent()),
                delegate ()
                {
                    TargetingParameters targetParams =
                    new TargetingParameters()
                    {
                        canTargetPawns = false,
                        canTargetBuildings = false,
                        canTargetItems = true,
                        mapObjectTargetsMustBeAutoAttackable = false,
                        validator = (target) => target.HasThing && target.Thing is Corpse corpse && corpse.InnerPawn.IsValidGenomeSequencingTarget()
                    };

                    Find.Targeter.BeginTargeting(targetParams,
                        delegate (LocalTargetInfo target)
                        {
                            Corpse corpse = target.Thing as Corpse;
                            if (corpse != null && selPawn.CanReserveAndReach(parent, PathEndMode.OnCell, Danger.Deadly) && selPawn.CanReserveAndReach(target, PathEndMode.OnCell, Danger.Deadly))
                            {
                                selPawn.jobs.TryTakeOrderedJob(new Job(SalvagerProps.salvagingJob, corpse, parent)
                                {
                                    count = 1
                                });
                            }
                        },
                        caster: selPawn);
                });
        }
    }
}
