using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace QEthics
{
    public class RecipeWorker_GenomeSequencing : RecipeWorker
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            if (pawn.IsValidGenomeSequencingTarget())
            {
                yield return null;
            }
            yield break;
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            RecipeOutcomeProperties props = ingredients?.FirstOrDefault()?.def.GetModExtension<RecipeOutcomeProperties>() ?? null;
            if(props != null)
            {
                Thing genomeSequence = GenomeUtility.MakeGenomeSequence(pawn, props.outputThingDef);
                GenPlace.TryPlaceThing(genomeSequence, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
            }
        }
    }
}
