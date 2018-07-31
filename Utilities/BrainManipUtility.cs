using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Harmony;
using System.Reflection;

namespace QEthics
{
    public static class BrainManipUtility
    {
        public static bool IsValidBrainScanningDef(this ThingDef def)
        {
            return !def.race.IsMechanoid && !GeneralCompatibility.excludedRaces.Contains(def);
        }

        public static bool IsValidBrainScanningTarget(this Pawn pawn)
        {
            ThingDef def = pawn.def;
            return IsValidBrainScanningDef(def) && !pawn.Dead && !pawn.health.hediffSet.hediffs.Any(hediff => GeneralCompatibility.excludedHediffs.Any(hediffDef => hediff.def == hediffDef));
        }

        public static bool IsValidBrainTemplatingTarget(this Pawn pawn)
        {
            return IsValidBrainScanningTarget(pawn) &&
                pawn.health.hediffSet.HasHediff(QEHediffDefOf.QE_CloneStatus, false) &&
                !pawn.health.hediffSet.HasHediff(QEHediffDefOf.QE_BrainTemplated, false);
        }

        public static Thing MakeBrainScan(Pawn pawn, ThingDef genomeDef)
        {
            Thing brainScanThing = ThingMaker.MakeThing(genomeDef);
            BrainScanTemplate brainScan = brainScanThing as BrainScanTemplate;
            if (brainScan != null)
            {
                //Standard.
                brainScan.sourceName = pawn?.Name?.ToStringFull ?? null;

                //Backgrounds
                Pawn_StoryTracker story = pawn.story;
                if (story != null)
                {
                    brainScan.backStoryChild = story.childhood;
                    brainScan.backStoryAdult = story.adulthood;
                }

                //Skills
                Pawn_SkillTracker skillTracker = pawn.skills;
                if(skillTracker != null)
                {
                    foreach (SkillRecord skill in skillTracker.skills)
                    {
                        brainScan.skills.Add(new SkillRecord()
                        {
                            def = skill.def,
                            Level = skill.Level,
                            passion = skill.passion
                        });
                    }
                }

                //Animal
                brainScan.isAnimal = pawn.RaceProps.Animal;

                //Training
                Pawn_TrainingTracker trainingTracker = pawn.training;
                if(trainingTracker != null)
                {
                    DefMap<TrainableDef, bool> learned = (DefMap<TrainableDef, bool>)AccessTools.Field(typeof(Pawn_TrainingTracker), "learned").GetValue(trainingTracker);
                    DefMap<TrainableDef, int> steps = (DefMap<TrainableDef, int>)AccessTools.Field(typeof(Pawn_TrainingTracker), "steps").GetValue(trainingTracker);

                    //Copy
                    foreach (var item in learned)
                    {
                        brainScan.trainingLearned[item.Key] = item.Value;
                    }
                    foreach (var item in steps)
                    {
                        brainScan.trainingSteps[item.Key] = item.Value;
                    }
                }
            }

            return brainScanThing;
        }

        public static void ApplyBrainScanTemplateOnPawn(Pawn pawn, BrainScanTemplate brainScan, float efficency = 1f)
        {
            if(pawn.IsValidBrainScanningTarget())
            {
                //Backgrounds
                Pawn_StoryTracker storyTracker = pawn.story;
                if (storyTracker != null)
                {
                    //story.childhood = brainScan.backStoryChild;
                    storyTracker.adulthood = brainScan.backStoryAdult;
                }

                //Skills
                Pawn_SkillTracker skillTracker = pawn.skills;
                if (skillTracker != null)
                {
                    foreach (SkillRecord skill in brainScan.skills)
                    {
                        SkillRecord pawnSkill = skillTracker.GetSkill(skill.def);
                        pawnSkill.Level = (int)Math.Floor((float)skill.levelInt * efficency);
                        pawnSkill.passion = skill.passion;
                        pawnSkill.Notify_SkillDisablesChanged();
                    }
                }

                //Dirty hack ahoy!
                if(storyTracker != null)
                {
                    AccessTools.Field(typeof(Pawn_StoryTracker), "cachedDisabledWorkTypes").SetValue(storyTracker, null);
                }

                //Training
                Pawn_TrainingTracker trainingTracker = pawn.training;
                if (trainingTracker != null)
                {
                    DefMap<TrainableDef, bool> learned = (DefMap<TrainableDef, bool>)AccessTools.Field(typeof(Pawn_TrainingTracker), "learned").GetValue(trainingTracker);
                    DefMap<TrainableDef, int> steps = (DefMap<TrainableDef, int>)AccessTools.Field(typeof(Pawn_TrainingTracker), "steps").GetValue(trainingTracker);

                    //Copy
                    foreach (var item in brainScan.trainingLearned)
                    {
                        learned[item.Key] = item.Value;
                    }
                    foreach (var item in brainScan.trainingSteps)
                    {
                        steps[item.Key] = (int)Math.Floor((float)item.Value * efficency);
                    }
                }

                //Apply Hediff
                pawn.health.AddHediff(QEHediffDefOf.QE_BrainTemplated);
            }
        }
    }
}
