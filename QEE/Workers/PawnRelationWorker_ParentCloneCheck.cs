using RimWorld;
using Verse;

namespace QEthics
{
    public class PawnRelationWorker_ParentCloneCheck : PawnRelationWorker_Parent
    {

        public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
        {
            //check if 'other' pawn is a Clone. If so, prevent the relation from happening
            if (other.health.hediffSet.HasHediff(QEHediffDefOf.QE_CloneStatus))
            {
                return;
            }
            else
            {
                base.CreateRelation(generated, other, ref request);
            }
        }

    }
}
