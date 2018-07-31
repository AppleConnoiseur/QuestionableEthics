using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace QEthics
{
    public class RecipeWorker_CreateBrainScan : RecipeWorker
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            if (pawn.IsValidBrainScanningTarget())
            {
                yield return null;
            }
            yield break;
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            RecipeOutcomeProperties props = ingredients?.FirstOrDefault()?.def.GetModExtension<RecipeOutcomeProperties>() ?? null;
            if (props != null)
            {
                Thing brainScan = BrainManipUtility.MakeBrainScan(pawn, props.outputThingDef);
                GenPlace.TryPlaceThing(brainScan, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
            }

            //Give headache
            pawn.health.AddHediff(QEHediffDefOf.QE_Headache, pawn.health.hediffSet.GetBrain());
        }
    }
}
