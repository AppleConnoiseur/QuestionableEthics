using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace QEthics
{
    [StaticConstructorOnStartup]
    public static class PostDefFixer
    {
        static PostDefFixer()
        {
            //Add recipes to valid Genome Sequencing targets.
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(def => def.category == ThingCategory.Pawn))
            {
                if(def.GetModExtension<RaceExclusionProperties>() is RaceExclusionProperties props)
                {
                    if(props.excludeThisRace)
                    {
                        GeneralCompatibility.excludedRaces.Add(def);
                    }

                    if(props.excludeTheseHediffs.Count > 0)
                    {
                        GeneralCompatibility.excludedHediffs.AddRange(props.excludeTheseHediffs);
                    }
                }

                if (def.IsValidGenomeSequencingTargetDef())
                {
                    if(def.recipes == null)
                    {
                        def.recipes = new List<RecipeDef>();
                    }
                    if (def.recipes.Count > 0)
                    {
                        def.recipes.Insert(0, QERecipeDefOf.QE_GenomeSequencing);
                    }
                    else
                    {
                        def.recipes.Add(QERecipeDefOf.QE_GenomeSequencing);
                    }
                }

                if(def.IsValidBrainScanningDef())
                {
                    if (def.recipes == null)
                    {
                        def.recipes = new List<RecipeDef>();
                    }
                    if (def.recipes.Count > 0)
                    {
                        def.recipes.Insert(0, QERecipeDefOf.QE_BrainScanning);
                    }
                    else
                    {
                        def.recipes.Add(QERecipeDefOf.QE_BrainScanning);
                    }
                }
            }

            //Inject our own backstories.
            foreach(BackstoryDef def in DefDatabase<BackstoryDef>.AllDefs)
            {
                Backstory backstory = new Backstory();
                backstory.slot = def.slot;
                backstory.title = def.title;
                backstory.titleShort = def.titleShort;
                backstory.titleFemale = def.titleFemale;
                backstory.titleShortFemale = def.titleShortFemale;
                backstory.baseDesc = def.baseDesc;
                AccessTools.Field(typeof(Backstory), "bodyTypeFemale").SetValue(backstory, def.bodyTypeFemale);
                AccessTools.Field(typeof(Backstory), "bodyTypeMale").SetValue(backstory, def.bodyTypeMale);
                AccessTools.Field(typeof(Backstory), "bodyTypeGlobal").SetValue(backstory, def.bodyTypeGlobal);
                backstory.spawnCategories.AddRange(def.spawnCategories);
                backstory.PostLoad();
                backstory.ResolveReferences();

                BackstoryDatabase.AddBackstory(backstory);

                def.identifier = backstory.identifier;
                //Log.Message("'" + def.defName + "' identifier is '" + backstory.identifier + "'");
            }
        }
    }
}
