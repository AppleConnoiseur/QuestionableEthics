using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.Sound;
using Verse.AI;

namespace QEthics
{
    /// <summary>
    /// A template of a pawns brain. Stores skills and backstories.
    /// </summary>
    public class BrainScanTemplate : ThingWithComps
    {
        public string sourceName = null;

        //Humanoid only
        public Backstory backStoryChild;
        public Backstory backStoryAdult;
        public List<SkillRecord> skills = new List<SkillRecord>();

        //Animals only
        public bool isAnimal;
        public DefMap<TrainableDef, bool> trainingLearned = new DefMap<TrainableDef, bool>();
        public DefMap<TrainableDef, int> trainingSteps = new DefMap<TrainableDef, int>();

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref sourceName, "sourceName");

            string childhoodIdentifier = (backStoryChild == null) ? null : backStoryChild.identifier;
            Scribe_Values.Look(ref childhoodIdentifier, "backStoryChild");
            if (Scribe.mode == LoadSaveMode.LoadingVars && !childhoodIdentifier.NullOrEmpty())
            {
                if (!BackstoryDatabase.TryGetWithIdentifier(childhoodIdentifier, out backStoryChild, true))
                {
                    Log.Error("Couldn't load child backstory with identifier " + childhoodIdentifier + ". Giving random.", false);
                    backStoryChild = BackstoryDatabase.RandomBackstory(BackstorySlot.Childhood);
                }
            }

            string adulthoodIdentifier = (backStoryAdult == null) ? null : backStoryAdult.identifier;
            Scribe_Values.Look(ref adulthoodIdentifier, "backStoryAdult");
            if (Scribe.mode == LoadSaveMode.LoadingVars && !adulthoodIdentifier.NullOrEmpty())
            {
                if (!BackstoryDatabase.TryGetWithIdentifier(adulthoodIdentifier, out backStoryAdult, true))
                {
                    Log.Error("Couldn't load adult backstory with identifier " + adulthoodIdentifier + ". Giving random.", false);
                    backStoryAdult = BackstoryDatabase.RandomBackstory(BackstorySlot.Adulthood);
                }
            }
            
            Scribe_Collections.Look(ref skills, "skills", LookMode.Deep);
            Scribe_Values.Look(ref isAnimal, "isAnimal");
            Scribe_Deep.Look(ref trainingLearned, "trainingLearned");
            Scribe_Deep.Look(ref trainingSteps, "trainingSteps");
        }

        public override string LabelNoCount
        {
            get
            {
                if(GetComp<CustomNameComp>() is CustomNameComp nameComp && nameComp.customName.NullOrEmpty())
                {
                    if (sourceName != null)
                    {
                        return sourceName + " " + base.LabelNoCount;
                    }
                    else
                    {
                        return base.LabelNoCount;
                    }
                }

                return base.LabelNoCount;
            }
        }

        public override string DescriptionDetailed
        {
            get
            {
                StringBuilder builder = new StringBuilder(base.DescriptionDetailed);

                builder.AppendLine();
                builder.AppendLine();
                if (sourceName != null)
                {
                    builder.AppendLine("QE_GenomeSequencerDescription_Name".Translate() + ": " + sourceName);
                }
                if (backStoryChild != null)
                    builder.AppendLine("QE_BrainScanDescription_BackshortChild".Translate() + ": " + backStoryChild.title.CapitalizeFirst());
                if (backStoryAdult != null)
                    builder.AppendLine("QE_BrainScanDescription_BackshortAdult".Translate() + ": " + backStoryAdult.title.CapitalizeFirst());

                //Skills
                if (!isAnimal && skills.Count > 0)
                {
                    builder.AppendLine("QE_BrainScanDescription_Skills".Translate());
                    foreach (SkillRecord skill in skills.OrderBy(skillRecord => skillRecord.def.index))
                    {
                        builder.AppendLine("    " + skill.def.LabelCap + ": " + skill.levelInt);
                    }
                }

                if (isAnimal)
                {
                    builder.AppendLine("QE_BrainScanDescription_Training".Translate());
                    foreach (var training in trainingSteps.OrderBy(trainingPair => trainingPair.Key.index))
                    {
                        builder.AppendLine("    " + training.Key.LabelCap + ": " + training.Value);
                    }
                }

                return builder.ToString().TrimEndNewlines();
            }
        }

        public override string DescriptionFlavor
        {
            get
            {
                StringBuilder builder = new StringBuilder(base.DescriptionFlavor);

                builder.AppendLine();
                builder.AppendLine();
                if (sourceName != null)
                {
                    builder.AppendLine("QE_GenomeSequencerDescription_Name".Translate() + ": " + sourceName);
                }
                if (backStoryChild != null)
                    builder.AppendLine("QE_BrainScanDescription_BackshortChild".Translate() + ": " + backStoryChild.title.CapitalizeFirst());
                if(backStoryAdult != null)
                    builder.AppendLine("QE_BrainScanDescription_BackshortAdult".Translate() + ": " + backStoryAdult.title.CapitalizeFirst());

                //Skills
                if (!isAnimal && skills.Count > 0)
                {
                    builder.AppendLine("QE_BrainScanDescription_Skills".Translate());
                    foreach (SkillRecord skill in skills.OrderBy(skillRecord => skillRecord.def.index))
                    {
                        builder.AppendLine("    " + skill.def.LabelCap + ": " + skill.levelInt);
                    }
                }

                if (isAnimal)
                {
                    builder.AppendLine("QE_BrainScanDescription_Training".Translate());
                    foreach (var training in trainingSteps.OrderBy(trainingPair => trainingPair.Key.index))
                    {
                        builder.AppendLine("    " + training.Key.LabelCap + ": " + training.Value);
                    }
                }

                return builder.ToString().TrimEndNewlines();
            }
        }

        public override Thing SplitOff(int count)
        {
            Thing splitThing = base.SplitOff(count);

            if(splitThing != this && splitThing is BrainScanTemplate brainScan)
            {
                //Shared
                brainScan.sourceName = sourceName;

                //Humanoid
                brainScan.backStoryChild = backStoryChild;
                brainScan.backStoryAdult = backStoryAdult;
                foreach (SkillRecord skill in skills)
                {
                    brainScan.skills.Add(new SkillRecord()
                    {
                        def = skill.def,
                        Level = skill.Level,
                        passion = skill.passion
                    });
                }

                //Animal
                foreach (var item in trainingLearned)
                {
                    brainScan.trainingLearned[item.Key] = item.Value;
                }
                foreach (var item in trainingSteps)
                {
                    brainScan.trainingSteps[item.Key] = item.Value;
                }
            }

            return splitThing;
        }

        public override bool CanStackWith(Thing other)
        {
            if (other is BrainScanTemplate brainScan &&
                backStoryChild == brainScan.backStoryChild &&
                backStoryAdult == brainScan.backStoryAdult &&
                DefMapsEqual(trainingLearned, brainScan.trainingLearned) &&
                DefMapsEqual(trainingSteps, brainScan.trainingSteps))
            {
                return base.CanStackWith(other);
            }

            return false;
        }

        public bool DefMapsEqual<T>(DefMap<TrainableDef, T> mapA, DefMap<TrainableDef,T> mapB) where T: new()
        {
            if(mapA.Count != mapB.Count)
            {
                return false;
            }

            foreach(var pair in mapA)
            {
                var validPairs = mapB.Where(pairB => pairB.Key == pair.Key);
                if(validPairs != null && validPairs.Count() > 0)
                {
                    var pairB = validPairs.First();
                    if(!pair.Value.Equals(pairB.Value))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (var option in base.GetFloatMenuOptions(selPawn))
            {
                yield return option;
            }

            //Start targeter
            yield return new FloatMenuOption("QE_BrainScanningApplyTemplate".Translate(),
                delegate()
                {
                    TargetingParameters targetParams = 
                    new TargetingParameters()
                    {
                        canTargetPawns = true,
                        validator = (target) => target.HasThing && target.Thing is Pawn pawn && pawn != selPawn && pawn.IsValidBrainTemplatingTarget()
                    };

                    Find.Targeter.BeginTargeting(targetParams, 
                        delegate(LocalTargetInfo target)
                        {
                            Pawn pawn = target.Thing as Pawn;
                            if(pawn != null)
                            {
                                Building_Bed validBed = pawn.FindSuitableSurgeryBed(selPawn);
                                if (validBed != null && selPawn.CanReserveAndReach(pawn, PathEndMode.OnCell, Danger.Deadly) && selPawn.CanReserveAndReach(this, PathEndMode.OnCell, Danger.Deadly) && selPawn.CanReserveAndReach(validBed, PathEndMode.OnCell, Danger.Deadly))
                                {
                                    selPawn.jobs.TryTakeOrderedJob(new Job(QEJobDefOf.QE_ApplyBrainScanTemplate, pawn, this, validBed)
                                    {
                                        count = 1
                                    });
                                }
                                else
                                {
                                    Messages.Message("QE_BrainScanningRejectInput".Translate(pawn.Name), MessageTypeDefOf.RejectInput, false);
                                    SoundDefOf.ClickReject.PlayOneShot(SoundInfo.OnCamera());
                                }
                            }
                        },
                        caster: selPawn);
                });

            //Find valid brain scanning target and medical bed on the map.
            /*var eligibleTargets = Map.mapPawns.FreeColonistsAndPrisonersSpawned.Where(pawn => pawn.IsValidBrainTemplatingTarget());
            if(eligibleTargets != null)
            {
                foreach(Pawn pawn in eligibleTargets)
                {
                    Building_Bed validBed = RestUtility.FindPatientBedFor(pawn);

                    bool disabled = validBed == null;
                    string label = null;
                    if(disabled)
                    {
                        label = "QE_BrainScanningFloatMenuLabelDisabled".Translate(pawn.LabelCap);
                    }
                    else
                    {
                        label = "QE_BrainScanningFloatMenuLabel".Translate(pawn.LabelCap);
                    }

                    FloatMenuOption option = new FloatMenuOption(label, delegate()
                    {

                    });
                    option.Disabled = disabled;

                    yield return option;
                }
            }*/
        }
    }
}
