using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace QEthics
{
    public static class GenomeUtility
    {
        public static Thing MakeGenomeSequence(Pawn pawn, ThingDef genomeDef)
        {
            Thing genomeThing = ThingMaker.MakeThing(genomeDef);
            GenomeSequence genomeSequence = genomeThing as GenomeSequence;
            if(genomeSequence != null)
            {
                //Standard.
                genomeSequence.sourceName = pawn?.Name?.ToStringFull ?? null;
                if(genomeSequence.sourceName == null)
                {
                    genomeSequence.sourceName = pawn.def.LabelCap;
                }
                genomeSequence.pawnKindDef = pawn.kindDef;
                genomeSequence.gender = pawn.gender;

                //Humanoid only.
                Pawn_StoryTracker story = pawn.story;
                if(story != null)
                {
                    genomeSequence.bodyType = story.bodyType;
                    genomeSequence.crownType = story.crownType;
                    genomeSequence.hairColor = story.hairColor;
                    genomeSequence.skinMelanin = story.melanin;
                    genomeSequence.hair = story.hairDef;

                    foreach (Trait trait in story.traits.allTraits)
                    {
                        genomeSequence.traits.Add(new ExposedTraitEntry(trait));
                    }
                }

                //Alien Races compatibility.
                if(CompatibilityTracker.AlienRacesActive)
                {
                    AlienRaceCompat.GetFieldsFromAlienComp(pawn, genomeSequence);
                }
            }

            return genomeThing;
        }

        public static Pawn MakePawnFromGenomeSequence(GenomeSequence genomeSequence, Thing creator)
        {
            //int adultAge = (int)genome.pawnKindDef.RaceProps.lifeStageAges.Last().minAge;

            PawnGenerationRequest request = new PawnGenerationRequest(
                genomeSequence.pawnKindDef,
                faction: creator.Faction,
                forceGenerateNewPawn: true,
                canGeneratePawnRelations: false,
                fixedGender: genomeSequence.gender,
                fixedBiologicalAge: 0,
                fixedChronologicalAge: 0,
                allowFood: false);
            Pawn pawn = PawnGenerator.GeneratePawn(request);

            //No pregenerated equipment.
            pawn?.equipment?.DestroyAllEquipment();
            pawn?.apparel?.DestroyAll();
            pawn?.inventory?.DestroyAll();

            //No pregenerated hediffs.
            pawn.health.hediffSet.Clear();

            //Set everything else.
            if (pawn.story is Pawn_StoryTracker storyTracker)
            {
                storyTracker.bodyType = genomeSequence.bodyType;
                storyTracker.crownType = genomeSequence.crownType;
                storyTracker.hairColor = genomeSequence.hairColor;
                storyTracker.hairDef = genomeSequence.hair ?? PawnHairChooser.RandomHairDefFor(pawn, pawn.Faction.def);
                storyTracker.melanin = genomeSequence.skinMelanin;

                storyTracker.traits.allTraits.Clear();
                foreach (ExposedTraitEntry trait in genomeSequence.traits)
                {
                    //storyTracker.traits.GainTrait(new Trait(trait.def, trait.degree));
                    storyTracker.traits.allTraits.Add(new Trait(trait.def, trait.degree));
                    if (pawn.workSettings != null)
                    {
                        pawn.workSettings.Notify_GainedTrait();
                    }
                    if (pawn.skills != null)
                    {
                        pawn.skills.Notify_SkillDisablesChanged();
                    }
                    if (!pawn.Dead && pawn.RaceProps.Humanlike)
                    {
                        pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
                    }
                }

                //Give random vatgrown backstory.
                storyTracker.childhood = DefDatabase<BackstoryDef>.GetNamed("Backstory_ColonyVatgrown").GetFromDatabase();
                storyTracker.adulthood = null;

                //Dirty hack ahoy!
                AccessTools.Field(typeof(Pawn_StoryTracker), "cachedDisabledWorkTypes").SetValue(storyTracker, null);
                //typeof(Pawn_StoryTracker).GetField("cachedDisabledWorkTypes", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(storyTracker, null);
                //typeof(PawnGenerator).GetMethod("GenerateSkills", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod).Invoke(null, new object[] { pawn });
            }

            if(pawn.skills is Pawn_SkillTracker skillsTracker)
            {
                foreach (SkillRecord skill in skillsTracker.skills)
                {
                    skill.Level = 0;
                    skill.passion = Passion.None;
                    skill.Notify_SkillDisablesChanged();
                }

                
                List<SkillRecord> skillPassions = new List<SkillRecord>();
                int skillsPicked = 0;
                int iterations = 0;
                //Pick 4 random skills to give minor passions to.
                while (skillsPicked < 4 && iterations < 1000)
                {
                    SkillRecord randomSkill = skillsTracker.skills.RandomElement();
                    if(!skillPassions.Contains(randomSkill))
                    {
                        skillPassions.Add(randomSkill);
                        randomSkill.passion = Passion.Minor;
                        skillsPicked++;
                    }

                    iterations++;
                }

                skillsPicked = 0;
                iterations = 0;
                //Pick 2 random skills to give major passions to.
                while (skillsPicked < 2 && iterations < 1000)
                {
                    SkillRecord randomSkill = skillsTracker.skills.RandomElement();
                    if (!skillPassions.Contains(randomSkill))
                    {
                        skillPassions.Add(randomSkill);
                        randomSkill.passion = Passion.Major;
                        skillsPicked++;
                    }

                    iterations++;
                }
            }

            if(pawn.workSettings is Pawn_WorkSettings workSettings)
            {
                workSettings.EnableAndInitialize();
            }

            //Alien Races compatibility.
            if (CompatibilityTracker.AlienRacesActive)
            {
                AlienRaceCompat.SetFieldsToAlienComp(pawn, genomeSequence);
            }

            PortraitsCache.SetDirty(pawn);
            PortraitsCache.PortraitsCacheUpdate();

            //Add Hediff marking them as a clone.
            pawn.health.AddHediff(QEHediffDefOf.QE_CloneStatus);

            return pawn;
        }

        public static bool IsValidGenomeSequencingTargetDef(this ThingDef def)
        {
            return !def.race.IsMechanoid &&
                def.GetStatValueAbstract(StatDefOf.MeatAmount) > 0f &&
                //def.GetStatValueAbstract(StatDefOf.LeatherAmount) > 0f &&
                !GeneralCompatibility.excludedRaces.Contains(def);
        }

        public static bool IsValidGenomeSequencingTarget(this Pawn pawn)
        {
            return IsValidGenomeSequencingTargetDef(pawn.def) && !pawn.health.hediffSet.hediffs.Any(hediff => GeneralCompatibility.excludedHediffs.Any(hediffDef => hediff.def == hediffDef));
        }
    }
}
