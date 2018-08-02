using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;

namespace QEthics
{
    /// <summary>
    /// A medical bed that automatically perform surgeries and healing.
    /// </summary>
    public class Building_AutoDoctor : Building_Bed
    {
        /// <summary>
        /// This is never saved. Destroyed upon on despawn.
        /// </summary>
        public Pawn tempSurgeonPawn;

        public CompRefuelable RefueableComp
        {
            get
            {
                return this.TryGetComp<CompRefuelable>();
            }
        }

        public override void Tick()
        {
            base.Tick();

            //Check all pawns in the bed.
            for(int i = 0; i < SleepingSlotsCount; i++)
            {
                IntVec3 sleepingSpot = GetSleepingSlotPos(i);
                Pawn pawn = sleepingSpot.GetFirstPawn(Map);
                if(pawn != null && pawn.CurrentlyUsableForBills())
                {
                    //Go through the Bills and find one we can do.
                    if(pawn.BillStack.AnyShouldDoNow)
                    {
                        float searchRadius = def.specialDisplayRadius;
                        List<Thing> foundIngredients = new List<Thing>();
                        int matchingIngredients = 0;
                        bool foundMatchingBill = false;

                        foreach (Bill bill in pawn.BillStack.Bills)
                        {
                            if(foundMatchingBill)
                            {
                                break;
                            }

                            foundIngredients.Clear();
                            matchingIngredients = 0;

                            foreach (IngredientCount ingredient in bill.recipe.ingredients)
                            {
                                if (!ingredient.filter.Allows(RefueableComp.Props.fuelFilter.AnyAllowedDef))
                                {
                                    //Look for ingredient in radius.
                                    Thing ingredientThing = GenClosest.ClosestThingReachable(Position, Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), Verse.AI.PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors),
                                        maxDistance: searchRadius,
                                        validator: thing => !thing.IsForbidden(Faction) && ingredient.filter.Allows(thing) && thing.stackCount >= ingredient.CountRequiredOfFor(thing.def, bill.recipe));

                                    if(ingredientThing != null)
                                    {
                                        //Found a valid ingredient to use.
                                        foundIngredients.Add(ingredientThing);
                                        matchingIngredients++;
                                    }
                                }
                                else
                                {
                                    //If its medicine, skip it.
                                    matchingIngredients++;
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            //Create temporary surgeon.
            if(tempSurgeonPawn == null)
            {
                PawnGenerationRequest request = new PawnGenerationRequest(
                    PawnKindDefOf.Colonist,
                    faction: Faction,
                    canGeneratePawnRelations: false,
                    forceGenerateNewPawn: true,
                    allowFood: false,
                    fixedBiologicalAge: 20,
                    fixedChronologicalAge: 20);
                tempSurgeonPawn = PawnGenerator.GeneratePawn(request);
                tempSurgeonPawn.Name = new NameTriple(LabelCap, LabelCap, "");
                tempSurgeonPawn.story.traits.allTraits.Clear();
                tempSurgeonPawn.skills.Notify_SkillDisablesChanged();
                tempSurgeonPawn.story.childhood = DefDatabase<BackstoryDef>.GetNamed("Backstory_ColonyVatgrown").GetFromDatabase();
                tempSurgeonPawn.story.adulthood = null;
                AccessTools.Field(typeof(Pawn_StoryTracker), "cachedDisabledWorkTypes").SetValue(tempSurgeonPawn.story, null);
                tempSurgeonPawn.skills.GetSkill(SkillDefOf.Medicine).Level = 20;
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);

            //Destroy and remove temporary surgeon.
            if(tempSurgeonPawn != null)
            {
                tempSurgeonPawn.Destroy();
                tempSurgeonPawn = null;
            }
        }
    }
}
