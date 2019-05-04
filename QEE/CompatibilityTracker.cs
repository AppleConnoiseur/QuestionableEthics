using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using System.Reflection;

namespace QEthics
{
    [StaticConstructorOnStartup]
    public static class CompatibilityTracker
    {
        private static bool alienRacesActiveInt = false;

        public static bool AlienRacesActive
        {
            get
            {
                return alienRacesActiveInt;
            }
        }

        private static bool deathRattleActiveInt = false;

        public static bool DeathRattleActive
        {
            get
            {
                return deathRattleActiveInt;
            }
        }

        static CompatibilityTracker()
        {
            //Check for Alien Races Compatiblity.
            Log.Message("Questionable Ethics checking for compatibility...");
            Log.Message("Checking for compatibility for: Alien Race Framework...");
            if(GenTypes.AllTypes.Any(type => type.FullName == "AlienRace.ThingDef_AlienRace"))
            {
                alienRacesActiveInt = true;
                Log.Message("Is OK.");
            }
            else
            {
                Log.Message("No compatibility.");
            }

            //Check for Death Rattle Compatiblity.
            Log.Message("Checking for compatibility for: Death Rattle...");
            if (GenTypes.AllTypes.Any(type => type.FullName == "DeathRattle.DeathRattleBase"))
            {
                deathRattleActiveInt = true;
                Log.Message("Is OK.");
            }
            else
            {
                Log.Message("No compatibility.");
            }
        }
    }
}
