using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QEthics
{
    public class RecipeWorker_NerveStapling : Recipe_InstallImplant
    {
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);

            //Check if the Hediff got applied.
            if (!pawn.health.hediffSet.HasHediff(recipe.addsHediff))
            {
                return;
            }

            //Convert to billDoer faction.
            bool isColonist = pawn.Faction == billDoer.Faction;
            if (pawn.Faction != billDoer.Faction)
            {
                pawn.SetFaction(billDoer.Faction, billDoer);
            }

            //Apply thoughts.
            pawn.needs.mood.thoughts.memories.TryGainMemory(QEThoughtDefOf.QE_RecentlyNerveStapled);
            pawn.needs.mood.thoughts.memories.TryGainMemory(QEThoughtDefOf.QE_NerveStapledMe, billDoer);

            foreach (Pawn thoughtReciever in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners)
            {
                if(thoughtReciever != pawn)
                {
                    if(isColonist)
                    {
                        thoughtReciever.needs.mood.thoughts.memories.TryGainMemory(QEThoughtDefOf.QE_NerveStapledColonist);
                    }
                    else
                    {
                        thoughtReciever.needs.mood.thoughts.memories.TryGainMemory(QEThoughtDefOf.QE_NerveStapledPawn);
                    }
                }
            }
        }
    }
}
