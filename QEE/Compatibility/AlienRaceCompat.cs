using AlienRace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using static AlienRace.AlienPartGenerator;

namespace QEthics
{
    public static class AlienRaceCompat
    {
        public static void Test()
        {
            Type type = typeof(ThingDef_AlienRace);
            Log.Message("type is: " + type.FullName);
        }

        public static void GetFieldsFromAlienComp(Pawn pawn, GenomeSequence genomeSequence)
        {
            AlienComp alienComp = pawn.TryGetComp<AlienComp>();
            if(alienComp != null)
            {
                genomeSequence.isAlien = true;
                genomeSequence.skinColor = alienComp.skinColor;
                genomeSequence.skinColorSecond = alienComp.skinColorSecond;
                genomeSequence.hairColorSecond = alienComp.hairColorSecond;
                genomeSequence.crownTypeAlien = alienComp.crownType;
            }
        }

        public static void SetFieldsToAlienComp(Pawn pawn, GenomeSequence genomeSequence)
        {
            AlienComp alienComp = pawn.TryGetComp<AlienComp>();
            if (alienComp != null)
            {
                alienComp.skinColor = genomeSequence.skinColor;
                alienComp.skinColorSecond = genomeSequence.skinColorSecond;
                alienComp.hairColorSecond = genomeSequence.hairColorSecond;
                alienComp.crownType = genomeSequence.crownTypeAlien;
            }
        }
    }
}
